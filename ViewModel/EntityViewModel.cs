using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
using Fork.Logic.WebRequesters;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Drawing.Image;
using Server = Fork.Logic.Model.Server;

namespace Fork.ViewModel;

public abstract class EntityViewModel : BaseViewModel
{
    public delegate void HandleEntityPathChangedEvent(object sender, EntityPathChangedEventArgs e);

    private int consoleMessagesLastSecond;
    private readonly List<ConsoleMessage> consoleOutListNoQuery;

    public ConsoleReader ConsoleReader;
    private List<double> cpuList;

    private CPUTracker cpuTracker;
    private string currentQuery = "";
    private List<double> diskList;

    private DiskTracker diskTracker;
    private bool isDeleted;
    private ConsoleMessage lastConsoleMessage = new("");
    private List<double> memList;

    private MemTracker memTracker;
    private double memValue;

    private ICommand readConsoleIn;

    public Task SettingsSavingTask = Task.CompletedTask;

    private readonly Stopwatch timeSinceLastConsoleMessage = new();

    protected EntityViewModel(Entity entity)
    {
        Entity = entity;
        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                consoleMessagesLastSecond = 0;
            }
        }) { IsBackground = true }.Start();

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

        FileInfo customIcon = new(Path.Combine(App.ServerPath, Entity.Name, "custom-icon.png"));
        if (!customIcon.Exists)
        {
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(
                new Uri("pack://application:,,,/View/Resources/images/Server-Icons/default.png")));
            using (FileStream fileStream = new(customIcon.FullName, FileMode.Create)) encoder.Save(fileStream);
        }

        List<string> iconUris = new()
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
                while (!ServerManager.Initialized) await Task.Delay(500);

                switch (this)
                {
                    case ServerViewModel serverViewModel:
                        await ServerManager.Instance.StartServerAsync(serverViewModel);
                        break;
                    case NetworkViewModel networkViewModel:
                        await ServerManager.Instance.StartNetworkAsync(networkViewModel,
                            networkViewModel.Network.SyncServers);
                        break;
                }
            });
        }

        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null && e.PropertyName.Equals(nameof(CurrentStatus)))
            {
                ApplicationManager.Instance.TriggerServerListEvent(sender, e);
            }
        };
    }

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

                EntitySerializer.Instance.StoreEntities();
            }) { IsBackground = true }.Start();
        }
    }

    public ObservableCollection<ConsoleMessage> ConsoleOutList { get; set; }

    public AppSettings AppSettings => AppSettingsSerializer.Instance.AppSettings;

    public string ConsoleIn { get; set; } = "";
    public ServerStatus CurrentStatus { get; set; }
    public bool ServerRunning => CurrentStatus == ServerStatus.RUNNING;

    public string AddressInfo { get; set; }

    public string CPUValue => Math.Round(CPUValueRaw, 0) + "%";
    public double CPUValueRaw { get; private set; }

    public string MemValue => Math.Round(memValue / Entity.JavaSettings.MaxRam * 100, 0) + "%";
    public double MemValueRaw => memValue / Entity.JavaSettings.MaxRam * 100;

    public string DiskValue => Math.Round(DiskValueRaw, 0) + "%";
    public double DiskValueRaw { get; private set; }

    public AvailabilityCheckResult? LastAvailabilityCheckResult { get; set; } = null;
    public bool IsAvailabilityCheckEnabled => ServerRunning && LastAvailabilityCheckResult != AvailabilityCheckResult.PENDING;

    public Brush AvailabilityColor
    {
        get
        {
            return LastAvailabilityCheckResult switch
            {
                AvailabilityCheckResult.OK => (Brush)new BrushConverter().ConvertFromString("#5EED80"),
                AvailabilityCheckResult.FAILED => (Brush)new BrushConverter().ConvertFromString("#ED5E5E"),
                AvailabilityCheckResult.PENDING => (Brush)new BrushConverter().ConvertFromString("#EBED78"),
                _ => (Brush)new BrushConverter().ConvertFromString("#565B7A")
            };
        }
    }

    [CanBeNull]
    public string AvailabilityTooltip
    {
        get
        {
            if (!IsAvailabilityCheckEnabled)
            {
                if (!ServerRunning)
                {
                    return $"Start the server to enable availability checks";
                }

                return $"Checking...";
            }

            return LastAvailabilityCheckResult switch
            {
                AvailabilityCheckResult.OK => "Server is available to users outside your network",
                AvailabilityCheckResult.FAILED =>
                    "Server is not reachable outside your network! Check your firewall and port forwarding rules",
                _ => null
            };
        }
    }

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
                case ServerStatus.RUNNING: return (Brush)new BrushConverter().ConvertFromString("#5EED80");
                case ServerStatus.STOPPED: return (Brush)new BrushConverter().ConvertFromString("#565B7A");
                case ServerStatus.STARTING: return (Brush)new BrushConverter().ConvertFromString("#EBED78");
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
                case ServerStatus.RUNNING: return (Brush)new BrushConverter().ConvertFromString("#5EED80");
                case ServerStatus.STOPPED: return (Brush)new BrushConverter().ConvertFromString("#1F2234");
                case ServerStatus.STARTING: return (Brush)new BrushConverter().ConvertFromString("#EBED78");
                default: return Brushes.White;
            }
        }
    }

    public ImageSource Icon
    {
        get
        {
            BitmapImage bi3 = new();
            bi3.BeginInit();
            switch (Entity.Version.Type)
            {
                case ServerVersion.VersionType.Vanilla:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Vanilla.png");
                    break;
                case ServerVersion.VersionType.Snapshot:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Snapshot.png");
                    break;
                case ServerVersion.VersionType.Paper:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Paper.png");
                    break;
                case ServerVersion.VersionType.Purpur:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Purpur.png");
                    break;
                case ServerVersion.VersionType.Spigot:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Spigot.png");
                    break;
                case ServerVersion.VersionType.Fabric:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Fabric.png");
                    break;
                case ServerVersion.VersionType.Waterfall:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/Waterfall.png");
                    break;
                case ServerVersion.VersionType.BungeeCord:
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
            BitmapImage bi3 = new();
            bi3.BeginInit();
            switch (Entity.Version.Type)
            {
                case ServerVersion.VersionType.Vanilla:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/VanillaW.png");
                    break;
                case ServerVersion.VersionType.Snapshot:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/SnapshotW.png");
                    break;
                case ServerVersion.VersionType.Paper:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/PaperW.png");
                    break;
                case ServerVersion.VersionType.Purpur:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/PurpurW.png");
                    break;
                case ServerVersion.VersionType.Spigot:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/SpigotW.png");
                    break;
                case ServerVersion.VersionType.Fabric:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/FabricW.png");
                    break;
                case ServerVersion.VersionType.Waterfall:
                    bi3.UriSource = new Uri("pack://application:,,,/View/Resources/images/Icons/WaterfallW.png");
                    break;
                case ServerVersion.VersionType.BungeeCord:
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

    public event HandleEntityPathChangedEvent EntityPathChangedEvent;

    public void UpdateCustomImage(string filePath)
    {
        if (isDeleted)
        {
            return;
        }

        try
        {
            ImageSource toRemove = ServerIcons.Last();
            bool newIsSelected = SelectedServerIcon == toRemove;
            Application.Current.Dispatcher?.Invoke(() => ServerIcons.Remove(toRemove));
            Bitmap bitmap;
            using (Image image = Image.FromFile(filePath)) bitmap = ImageUtils.ResizeImage(image, 64, 64);

            ImageSource img = ImageUtils.BitmapToImageSource(bitmap);
            img.Freeze();
            Application.Current.Dispatcher?.Invoke(() => ServerIcons.Add(img));
            if (newIsSelected)
            {
                SelectedServerIcon = img;
            }

            bitmap.Save(Path.Combine(App.ServerPath, Entity.Name, "custom-icon.png"));
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
        }
    }

    public void SaveSettings()
    {
        if (isDeleted)
        {
            return;
        }

        SettingsSavingTask = SettingsViewModel.SaveChanges();
        Task.Run(() => SettingsSavingTask);
        if (this is ServerViewModel serverViewModel)
        {
            ServerAutomationManager.Instance.UpdateAutomation(serverViewModel);
        }

        Task.Run(async () =>
        {
            WriteServerIcon();
            UpdateAddressInfo();
            //Update Network page if one exists where this server is in
            if (this is ServerViewModel serverViewModel)
            {
                foreach (EntityViewModel entityViewModel in ServerManager.Instance.Entities)
                    if (entityViewModel is NetworkViewModel networkViewModel)
                    {
                        foreach (NetworkServer networkServer in networkViewModel.Servers)
                            if (networkServer is NetworkForkServer networkForkServer)
                            {
                                if (networkForkServer.ServerViewModel == this)
                                {
                                    networkViewModel.UpdateServer(networkServer, serverViewModel);
                                }
                            }
                    }
            }

            EntitySerializer.Instance.StoreEntities();
        });
    }

    public void AddToConsole(ConsoleMessage message)
    {
        lock (this)
        {
            if (AppSettings.ConsoleThrottling && message.Level == ConsoleMessage.MessageLevel.INFO)
            {
                int threshold =
                    (int)Math.Round(Math.Min(lastConsoleMessage.Content.Length, message.Content.Length) * 0.10);
                int dist = StringUtils.DamerauLevenshteinDistance(lastConsoleMessage.Content,
                    message.Content, threshold);
                if (dist < threshold * 5 / Math.Max(timeSinceLastConsoleMessage.Elapsed.TotalSeconds, 1))
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
            EntitySerializer.Instance.StoreEntities();
        }
    }

    public void InitializeSettingsFiles(List<SettingsFile> files)
    {
        SettingsViewModel.InitializeSettings(files);
    }

    public async Task UpdateSettingsFiles(List<string> fileNames)
    {
        await SettingsViewModel.UpdateSettings(fileNames);
    }

    public void DeleteEntity()
    {
        isDeleted = true;
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
        try
        {
            cpuList.Add(value);
            if (cpuList.Count > 3)
            {
                cpuList.RemoveAt(0);
            }

            CPUValueRaw = cpuList.Average();
        }
        catch
        {
            //ignore, only errors rarely if collection was modified
        }

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

        try
        {
            memValue = memList.Average();
        }
        catch
        {
            //ignore, only errors rarely if collection was modified
        }

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

        try
        {
            DiskValueRaw = diskList.Average();
        }
        catch
        {
            //ignore, only errors rarely if collection was modified
        }

        raisePropertyChanged(nameof(DiskValue));
        raisePropertyChanged(nameof(DiskValueRaw));
    }

    public void StartDownload()
    {
        DownloadCompleted = false;
        Entity.Initialized = false;
        EntitySerializer.Instance.StoreEntities();
        Console.WriteLine($"Starting server.jar download for {Entity}");
        raisePropertyChanged(nameof(Server));
        raisePropertyChanged(nameof(ReadyToUse));
    }

    public void DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
    {
        double bytesIn = double.Parse(e.BytesReceived.ToString());
        double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
        DownloadProgress = bytesIn / totalBytes * 100;
    }

    public void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs e)
    {
        DownloadCompleted = true;
        Entity.Initialized = true;
        EntitySerializer.Instance.StoreEntities();
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
        CopyProgress = e.FilesCopied / (double)e.FilesToCopy * 100;
        //CopyProgressReadable = Math.Round(CopyProgress, 0) + "%";
    }

    public void ClearConsole()
    {
        Application.Current.Dispatcher.Invoke(() => ConsoleOutList.Clear());
    }

    private void ConsoleOutChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        raisePropertyChanged(nameof(ConsoleOutList));
    }

    protected void StartSettingsReader()
    {
        SettingsReader settingsReader = new(this);
        ApplicationManager.Instance.SettingsReaders.Add(settingsReader);
    }


    private void WriteServerIcon()
    {
        try
        {
            Entity.ServerIconId = ServerIcons.IndexOf(SelectedServerIcon);
            FileInfo customIcon = new(Path.Combine(App.ServerPath, Entity.Name, "server-icon.png"));
            //Remove the server-icon.png and return if the default icon is selected.
            if (Entity.ServerIconId == 0 && customIcon.Exists)
            {
                customIcon.Delete();
                return;
            }

            if (Entity.ServerIconId == 0)
            {
                return;
            }

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)SelectedServerIcon));
            using (FileStream fileStream = new(customIcon.FullName, FileMode.Create)) encoder.Save(fileStream);
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
        List<ConsoleMessage> original = new(ConsoleOutList);
        foreach (ConsoleMessage consoleMessage in original)
            if (!consoleMessage.Content.ToLower().Contains(query.ToLower()))
            {
                Application.Current.Dispatcher?.Invoke(() => ConsoleOutList.Remove(consoleMessage),
                    DispatcherPriority.Send);
            }
    }

    private void ResetConsoleOutList()
    {
        for (int i = 0; i < consoleOutListNoQuery.Count; i++)
        {
            if (ConsoleOutList.Count <= i || consoleOutListNoQuery[i] != ConsoleOutList[i])
            {
                int i1 = i;
                Application.Current.Dispatcher?.Invoke(() => ConsoleOutList.Insert(i1, consoleOutListNoQuery[i1]));
            }
        }
    }


    [NotifyPropertyChangedInvocator]
    protected void raisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        RaisePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public class EntityPathChangedEventArgs
    {
        public EntityPathChangedEventArgs(string newPath)
        {
            NewPath = newPath;
        }

        public string NewPath { get; }
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

    public enum AvailabilityCheckResult
    {
        OK, PENDING, FAILED
    }
}