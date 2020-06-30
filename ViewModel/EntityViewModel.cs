using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using fork.Annotations;
using fork.Logic.BackgroundWorker.Performance;
using fork.Logic.CustomConsole;
using fork.Logic.ImportLogic;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Model.Settings;
using fork.Logic.Persistence;
using Server = fork.Logic.Model.Server;

namespace fork.ViewModel
{
    public abstract class EntityViewModel : BaseViewModel
    {
        private CPUTracker cpuTracker;
        private List<double> cpuList;
        private double cpuValue;
        
        private MEMTracker memTracker;
        private List<double> memList;
        private double memValue;
        
        private DiskTracker diskTracker;
        private List<double> diskList;
        private double diskValue;
        
        public Entity Entity { get; set; }
        public ConsoleReader ConsoleReader;
        public ObservableCollection<string> ConsoleOutList { get; set; }
        
        public string ConsoleIn { get; set; } = "";
        public ServerStatus CurrentStatus { get; set; }
        public bool ServerRunning => CurrentStatus == ServerStatus.RUNNING;
        
        public string AddressInfo { get; set; }
        
        public string CPUValue => Math.Round(cpuValue,0) + "%";
        public double CPUValueRaw => cpuValue;

        public string MemValue => Math.Round(memValue/Entity.JavaSettings.MaxRam *100,0) + "%";
        public double MemValueRaw => memValue/Entity.JavaSettings.MaxRam *100;

        public string DiskValue => Math.Round(diskValue, 0) + "%";
        public double DiskValueRaw => diskValue;
        
        private ICommand readConsoleIn;

        public ICommand ReadConsoleIn
        {
            get
            {
                return readConsoleIn
                       ?? (readConsoleIn = new ActionCommand(() =>
                       {
                           ConsoleReader?.Read(ConsoleIn, this);
                           ConsoleIn = "";
                       }));
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
        
        public double DownloadProgress { get; set; }
        public bool DownloadCompleted { get; set; }
        public double CopyProgress { get; set; }
        public bool ImportCompleted { get; set; } = true;
        public bool ReadyToUse => Entity.Initialized && ImportCompleted;

        public Page EntityPage { get; set; }
        public Page ConsolePage { get; set; }
        
        public Page PluginsPage { get; set; }
        
        public SettingsViewModel SettingsViewModel { get; set; }


        public EntityViewModel(Entity entity)
        {
            Entity = entity;
            
            //Error weird crash (should not happen unless entities.json is corrupted)
            if (Entity.Version == null)
            {
                Console.WriteLine("Persistence file storing servers probably is corrupted (entities.json). Can not start Fork!");
            }
            CurrentStatus = ServerStatus.STOPPED;
            ConsoleOutList = new ObservableCollection<string>();
            ConsoleOutList.CollectionChanged += ConsoleOutChanged;
            
            new Thread(() =>
            {
                Thread.Sleep(1000);
                Application.Current.Dispatcher?.Invoke(StartSettingsReader);
            }).Start();
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
            memTracker = new MEMTracker();
            memTracker.TrackP(p, this);
            
            
            // Track disk usage
            diskTracker?.StopThreads();
            
            diskList = new List<double>();
            diskTracker = new DiskTracker();
            diskTracker.TrackTotal(p,this);
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
            Console.WriteLine("Starting server.jar download for server "+Entity);
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
            CopyProgress = (double)e.FilesCopied / (double)e.FilesToCopy *100;
            //CopyProgressReadable = Math.Round(CopyProgress, 0) + "%";
        }
        
        private void ConsoleOutChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(ConsoleOutList));
        }
        
        private void StartSettingsReader()
        {
            SettingsReader settingsReader = new SettingsReader(this);
            ApplicationManager.Instance.SettingsReaders.Add(settingsReader);
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