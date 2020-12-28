using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using Fork.Annotations;
using Fork.Logic.BackgroundWorker.Performance;
using Fork.Logic.Controller;
using Fork.Logic.CustomConsole;
using Fork.Logic.ImportLogic;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.ProxyModels;
using Fork.Logic.Model.ServerConsole;
using Fork.Logic.Model.Settings;
using Fork.Logic.Persistence;
using Fork.Logic.Utils;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Drawing.Image;
using Server = Fork.Logic.Model.Server;

namespace Fork.ViewModel
{
    public abstract class EntityViewModel : BaseViewModel
    {
        private List<ConsoleMessage> consoleOutListNoQuery;
        private string currentQuery = "";
        private int consoleMessagesLastSecond = 0;
        private ConsoleMessage lastConsoleMessage = new ConsoleMessage("");

        private CPUTracker cpuTracker;
        private List<double> cpuList;
        private double cpuValue;

        private MemTracker memTracker;
        private List<double> memList;
        private double memValue;

        private DiskTracker diskTracker;
        private List<double> diskList;
        private double diskValue;
        
        private Stopwatch timeSinceLastConsoleMessage = new Stopwatch();

        public class EntityPathChangedEventArgs
        {
            public string NewPath { get; }

            public EntityPathChangedEventArgs(string newPath)
            {
                NewPath = newPath;
            }
        }

        public delegate void HandleEntityPathChangedEvent(object sender, EntityPathChangedEventArgs e);

        public event HandleEntityPathChangedEvent EntityPathChangedEvent;

        public Entity Entity { get; set; }

        public string Name
        {
            get => Entity.Name;
            set
            {
                Entity.Name = value;
                EntityPathChangedEvent?.Invoke(this,
                    new EntityPathChangedEventArgs(Path.Combine(App.ServerPath, value)));
                new Thread(() =>
                {
                    if (this is ServerViewModel s)
                    {
                        raisePropertyChanged(nameof(s.ServerTitle));
                    }
                    else if (this is NetworkViewModel n)
                    {
                        raisePropertyChanged(nameof(n.NetworkTitle));
                    }

                    EntitySerializer.Instance.StoreEntities(ServerManager.Instance.Entities);
                }) {IsBackground = true}.Start();
            }
        }

        public ConsoleReader ConsoleReader;
        public ObservableCollection<ConsoleMessage> ConsoleOutList { get; set; }

        public AppSettings AppSettings => AppSettingsSerializer.Instance.AppSettings;

        public string ConsoleIn { get; set; } = "";
        public ServerStatus CurrentStatus { get; set; }
        public bool ServerRunning => CurrentStatus == ServerStatus.RUNNING;

        public string AddressInfo { get; set; }

        public string CPUValue => Math.Round(cpuValue, 0) + "%";
        public double CPUValueRaw => cpuValue;

        public string MemValue => Math.Round(memValue / Entity.JavaSettings.MaxRam * 100, 0) + "%";
        public double MemValueRaw => memValue / Entity.JavaSettings.MaxRam * 100;

        public string DiskValue => Math.Round(diskValue, 0) + "%";
        public double DiskValueRaw => diskValue;

        private ICommand readConsoleIn;

        public ICommand ReadConsoleIn
        {
            get
            {
                return readConsoleIn ??= new ActionCommand(() =>
                {
                    ConsoleReader?.Read(ConsoleIn, this);
                    ConsoleIn = "";
                });
            }
        }

        public Brush IconColor
        {
            get
            {
                switch (CurrentStatus)
                {
                    case ServerStatus.RUNNING: return (Brush) new BrushConverter().ConvertFromString("#5EED80");
                    case ServerStatus.STOPPED: return (Brush) new BrushConverter().ConvertFromString("#565B7A");
                    case ServerStatus.STARTING: return (Brush) new BrushConverter().ConvertFromString("#EBED78");
                    default: return Brushes.White;
                }
            }
        }

        public Brush IconColorHovered
        {
            get
            {
                switch (CurrentStatus)
                {
                    case ServerStatus.RUNNING: return (Brush) new BrushConverter().ConvertFromString("#5EED80");
                    case ServerStatus.STOPPED: return (Brush) new BrushConverter().ConvertFromString("#1F2234");
                    case ServerStatus.STARTING: return (Brush) new BrushConverter().ConvertFromString("#EBED78");
                    default: return Brushes.White;
                }
            }
        }

        public ImageSource Icon
        {
            get
            {
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                switch (Entity.Version.Type)
                {
                    case ServerVersion.VersionType.Vanilla:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Vanilla.png");
                        break;
                    case ServerVersion.VersionType.Paper:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Paper.png");
                        break;
                    case ServerVersion.VersionType.Spigot:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Spigot.png");
                        break;
                    case ServerVersion.VersionType.Waterfall:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Waterfall.png");
                        break;
                    default:
                        return null;
                }

                bi3.EndInit();
                return bi3;
            }
        }

        public ImageSource IconW
        {
            get
            {
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                switch (Entity.Version.Type)
                {
                    case ServerVersion.VersionType.Vanilla:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/VanillaW.png");
                        break;
                    case ServerVersion.VersionType.Paper:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/PaperW.png");
                        break;
                    case ServerVersion.VersionType.Spigot:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/SpigotW.png");
                        break;
                    case ServerVersion.VersionType.Waterfall:
                        bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/WaterfallW.png");
                        break;
                    default:
                        return null;
                }

                bi3.EndInit();
                return bi3;
            }
        }

        public ObservableCollection<ImageSource> ServerIcons { get; set; }

        public ImageSource SelectedServerIcon { get; set; }

        public double DownloadProgress { get; set; }
        public bool DownloadCompleted { get; set; }
        public double CopyProgress { get; set; }
        public bool ImportCompleted { get; set; } = true;
        public bool ReadyToUse => Entity.Initialized && ImportCompleted;

        public Page EntityPage { get; set; }
        public Page ConsolePage { get; set; }

        public Page PluginsPage { get; set; }

        public SettingsViewModel SettingsViewModel { get; set; }


        protected EntityViewModel(Entity entity)
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    consoleMessagesLastSecond = 0;
                }
            }) {IsBackground = true}.Start();

            Entity = entity;

            //Error weird crash (should not happen unless entities.json is corrupted)
            //TODO check for json errors in entities.json
            if (Entity.Version == null)
            {
                Console.WriteLine(
                    "Persistence file storing servers probably is corrupted (entities.json). Can not start Fork!");
            }

            CurrentStatus = ServerStatus.STOPPED;
            consoleOutListNoQuery = new List<ConsoleMessage>();
            ConsoleOutList = new ObservableCollection<ConsoleMessage>();
            ServerIcons = new ObservableCollection<ImageSource>();

            FileInfo customIcon = new FileInfo(Path.Combine(App.ServerPath, Entity.Name, "custom-icon.png"));
            if (!customIcon.Exists)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(
                    new Uri("pack://application:,,,/View/Resources/images/Server-Icons/default.png")));
                using (FileStream fileStream = new FileStream(customIcon.FullName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }

            List<string> iconUris = new List<string>
            {
                "pack://application:,,,/View/Resources/images/Server-Icons/default.png",
                "pack://application:,,,/View/Resources/images/Server-Icons/forkboi.png",
                "pack://application:,,,/View/Resources/images/Server-Icons/forkchristmas.png",
                "pack://application:,,,/View/Resources/images/Server-Icons/icon1.png",
                customIcon.FullName
            };
            foreach (string iconUri in iconUris)
            {
                Image image;
                if (iconUri.StartsWith("pack://application:,,,/"))
                {
                    StreamResourceInfo info = Application.GetResourceStream(new Uri(iconUri));
                    image = Image.FromStream(info?.Stream);
                }
                else
                {
                    image = Image.FromFile(iconUri);
                }
                
                Bitmap bitmap = ImageUtils.ResizeImage(image, 64, 64);
                ImageSource img = ImageUtils.BitmapToImageSource(bitmap);
                img.Freeze();
                Application.Current.Dispatcher?.Invoke(() => ServerIcons.Add(img));
            }
            if (Entity.ServerIconId >= 0 && Entity.ServerIconId < ServerIcons.Count)
            {
                SelectedServerIcon = ServerIcons[Entity.ServerIconId];
            }
            else
            {
                SelectedServerIcon = ServerIcons[0];
                Entity.ServerIconId = 0;
            }
            WriteServerIcon();


            ConsoleOutList.CollectionChanged += ConsoleOutChanged;

            UpdateAddressInfo();

            if (Entity.StartWithFork)
            {
                Task.Run(async () =>
                {
                    while (!ServerManager.Initialized)
                    {
                        await Task.Delay(500);
                    }

                    switch (this)
                    {
                        case ServerViewModel serverViewModel:
                            await ServerManager.Instance.StartServerAsync(serverViewModel);
                            break;
                        case NetworkViewModel networkViewModel:
                            await ServerManager.Instance.StartNetworkAsync(networkViewModel);
                            break;
                    }
                });
            }
        }

        public void UpdateCustomImage(string filePath)
        {
            try
            {
                ImageSource toRemove = ServerIcons.Last();
                var newIsSelected = SelectedServerIcon == toRemove;
                Application.Current.Dispatcher?.Invoke(() => ServerIcons.Remove(toRemove));
                Bitmap bitmap;
                using (Image image = Image.FromFile(filePath))
                {
                    bitmap = ImageUtils.ResizeImage(image, 64, 64);
                }
                ImageSource img = ImageUtils.BitmapToImageSource(bitmap);
                img.Freeze();
                Application.Current.Dispatcher?.Invoke(() => ServerIcons.Add(img));
                if (newIsSelected)
                {
                    SelectedServerIcon = img;
                }
                bitmap.Save(Path.Combine(App.ServerPath,Entity.Name,"custom-icon.png"));
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        }

        public void SaveSettings()
        {
            new Thread(() =>
            {
                WriteServerIcon();
                UpdateAddressInfo();
                SettingsViewModel.SaveChanges();
                EntitySerializer.Instance.StoreEntities(ServerManager.Instance.Entities);
            }).Start();
        }

        public void AddToConsole(ConsoleMessage message)
        {
            lock (this)
            {
                if (message.Level == ConsoleMessage.MessageLevel.INFO)
                {
                    int threshold = (int) Math.Round(Math.Min(lastConsoleMessage.Content.Length, message.Content.Length) * 0.10);
                    int dist = StringUtils.DamerauLevenshteinDistance(lastConsoleMessage.Content, 
                        message.Content, threshold);
                    if (dist < (threshold*5)/Math.Max(timeSinceLastConsoleMessage.Elapsed.TotalSeconds,1))
                    {
                        lastConsoleMessage.SubContents++;
                        return;
                    }

                    if (consoleMessagesLastSecond > AppSettings.MaxConsoleLinesPerSecond)
                    {
                        return;
                    }
                } 

                try
                {
                    if (CurrentStatus == ServerStatus.RUNNING)
                    {
                        consoleMessagesLastSecond++;
                    }

                    lastConsoleMessage = message;
                    consoleOutListNoQuery.Add(message);
                    if (message.Content.Contains(currentQuery))
                    {
                        Application.Current?.Dispatcher?.Invoke(() => ConsoleOutList.Add(message),
                            DispatcherPriority.ApplicationIdle);
                        timeSinceLastConsoleMessage.Restart();
                    }

                    while (consoleOutListNoQuery.Count > AppSettings.MaxConsoleLines)
                    {
                        ConsoleMessage messageToDelete = consoleOutListNoQuery[0];
                        if (ConsoleOutList.Contains(messageToDelete))
                        {
                            Application.Current?.Dispatcher?.Invoke(() => ConsoleOutList.RemoveAt(0),
                                DispatcherPriority.ApplicationIdle);
                        }

                        consoleOutListNoQuery.RemoveAt(0);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while adding line to console: " + message);
                    ErrorLogger.Append(e);
                }
            }
        }

        public void ApplySearchQueryToConsole(string query)
        {
            if (query.Equals(""))
            {
                ResetConsoleOutList();
            }
            else if (query.StartsWith(currentQuery))
            {
                RemoveNotMatchingMessages(query);
            }
            else
            {
                ResetConsoleOutList();
                RemoveNotMatchingMessages(query);
            }

            currentQuery = query;
        }

        public void UpdateDefaultJavaPath(string oldPath, string newPath)
        {
            if (Entity.JavaSettings.JavaPath.Equals(oldPath))
            {
                Entity.JavaSettings.JavaPath = newPath;
            }
        }

        public void UpdateSettingsFiles(List<SettingsFile> files, bool initial = false)
        {
            if (initial)
            {
                SettingsViewModel.InitializeSettings(files);
            }
            else
            {
                SettingsViewModel.UpdateSettings(files);
            }
        }

        public void TrackPerformance(Process p)
        {
            // Track CPU usage
            cpuTracker?.StopThreads();

            cpuList = new List<double>();
            cpuTracker = new CPUTracker();
            cpuTracker.TrackTotal(p, this);


            // Track memory usage
            memTracker?.StopThreads();

            memList = new List<double>();
            memTracker = new MemTracker();
            memTracker.TrackP(p, this);


            // Track disk usage
            diskTracker?.StopThreads();

            diskList = new List<double>();
            diskTracker = new DiskTracker();
            diskTracker.TrackTotal(p, this);
        }


        public void CPUValueUpdate(double value)
        {
            cpuList.Add(value);
            if (cpuList.Count > 3)
            {
                cpuList.RemoveAt(0);
            }

            cpuValue = cpuList.Average();
            raisePropertyChanged(nameof(CPUValue));
            raisePropertyChanged(nameof(CPUValueRaw));
        }

        public void MemValueUpdate(double value)
        {
            memList.Add(value);
            if (memList.Count > 3)
            {
                memList.RemoveAt(0);
            }

            memValue = memList.Average();
            raisePropertyChanged(nameof(MemValue));
            raisePropertyChanged(nameof(MemValueRaw));
        }

        public void DiskValueUpdate(double value)
        {
            diskList.Add(value);
            if (diskList.Count > 3)
            {
                diskList.RemoveAt(0);
            }

            diskValue = diskList.Average();
            raisePropertyChanged(nameof(DiskValue));
            raisePropertyChanged(nameof(DiskValueRaw));
        }

        public void StartDownload()
        {
            DownloadCompleted = false;
            Entity.Initialized = false;
            EntitySerializer.Instance.StoreEntities(ServerManager.Instance.Entities);
            Console.WriteLine("Starting server.jar download for server " + Entity);
            raisePropertyChanged(nameof(Server));
            raisePropertyChanged(nameof(ReadyToUse));
        }

        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            DownloadProgress = bytesIn / totalBytes * 100;
            //DownloadProgressReadable = Math.Round(DownloadProgress, 0) + "%";
        }

        public void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs e)
        {
            DownloadCompleted = true;
            Entity.Initialized = true;
            EntitySerializer.Instance.StoreEntities(ServerManager.Instance.Entities);
            Console.WriteLine("Finished downloading server.jar for server " + Entity);
            raisePropertyChanged(nameof(Server));
            raisePropertyChanged(nameof(ReadyToUse));
        }

        public void StartImport()
        {
            ImportCompleted = false;
            raisePropertyChanged(nameof(ImportCompleted));
            raisePropertyChanged(nameof(ReadyToUse));
        }

        public void FinishedCopying()
        {
            ImportCompleted = true;
            if (this is ServerViewModel serverViewModel)
            {
                serverViewModel.InitializeLists(serverViewModel.Server);
            }

            raisePropertyChanged(nameof(ImportCompleted));
            raisePropertyChanged(nameof(ReadyToUse));
        }

        public void CopyProgressChanged(object sender, FileImporter.CopyProgressChangedEventArgs e)
        {
            CopyProgress = (double) e.FilesCopied / (double) e.FilesToCopy * 100;
            //CopyProgressReadable = Math.Round(CopyProgress, 0) + "%";
        }

        private void ConsoleOutChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(ConsoleOutList));
        }

        public void StartSettingsReader()
        {
            SettingsReader settingsReader = new SettingsReader(this);
            ApplicationManager.Instance.SettingsReaders.Add(settingsReader);
        }
        
        
        private void WriteServerIcon()
        {
            try
            {
                Entity.ServerIconId = ServerIcons.IndexOf(SelectedServerIcon);
                FileInfo customIcon = new FileInfo(Path.Combine(App.ServerPath, Entity.Name, "server-icon.png"));
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource) SelectedServerIcon));
                using (FileStream fileStream = new FileStream(customIcon.FullName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("Saving server icon failed! See error log");
            }
        }

        private void UpdateAddressInfo()
        {
            if (Entity is Server server)
            {
                AddressInfo = new APIController().GetExternalIPAddress() + ":" + server.ServerSettings.ServerPort;
            }
        }

        private void RemoveNotMatchingMessages(string query)
        {
            List<ConsoleMessage> original = new List<ConsoleMessage>(ConsoleOutList);
            foreach (ConsoleMessage consoleMessage in original)
            {
                if (!consoleMessage.Content.ToLower().Contains(query.ToLower()))
                {
                    Application.Current.Dispatcher?.Invoke(() => ConsoleOutList.Remove(consoleMessage),
                        DispatcherPriority.Send);
                }
            }
        }

        private void ResetConsoleOutList()
        {
            for (int i = 0; i < consoleOutListNoQuery.Count; i++)
            {
                if (ConsoleOutList.Count <= i || consoleOutListNoQuery[i] != ConsoleOutList[i])
                {
                    var i1 = i;
                    Application.Current.Dispatcher?.Invoke(() => ConsoleOutList.Insert(i1, consoleOutListNoQuery[i1]));
                }
            }
        }


        [NotifyPropertyChangedInvocator]
        protected void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute(object parameter)
            {
                _action();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }
    }
}