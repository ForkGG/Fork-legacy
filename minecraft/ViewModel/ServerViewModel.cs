using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using fork.Annotations;
using fork.Logic.BackgroundWorker.Performance;
using fork.Logic.CustomConsole;
using fork.Logic.ImportLogic;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Persistence;
using fork.Logic.RoleManagement;
using fork.View.Xaml2.Pages;
using Fork.View.Xaml2.Pages;
using Console = System.Console;
using RadioButton = System.Windows.Forms.RadioButton;
using Timer = System.Timers.Timer;

namespace fork.ViewModel
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        private string externalIP = new WebClient().DownloadString("http://icanhazip.com").Trim();

        private CPUTracker cpuTracker;
        private double cpuValue;
        private MEMTracker memTracker;
        private double memValue;
        private DiskTracker diskTracker;
        private double diskValue;

        private Timer restartTimer = null;

        private RoleUpdater whitelistUpdater;
        private RoleUpdater banlistUpdater;
        private RoleUpdater oplistUpdater;
        private World activeWorld;

        public ConsoleReader ConsoleReader;
        public ObservableCollection<string> ConsoleOutList { get; }
        public ObservableCollection<ServerPlayer> PlayerList { get; set; } = new ObservableCollection<ServerPlayer>();
        public ObservableCollection<Player> BanList { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<Player> OPList { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<Player> WhiteList { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<ServerVersion> Versions { get; set; } = new ObservableCollection<ServerVersion>();
        public ObservableCollection<World> Worlds { get; set; }
        
        public string ConsoleIn { get; set; } = "";
        public ServerStatus CurrentStatus { get; set; }
        public Server Server { get; set; }

        public bool RestartEnabled { get; set; }
        public string NextRestartHours { get; set; }
        public string NextRestartMinutes { get; set; }
        public string NextRestartSeconds { get; set; }

        public string ServerTitle => Server.Name + " - " + Server.Version.Type + " " + Server.Version.Version;

        public ImageSource Icon
        {
            get
            {
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                switch (Server.Version.Type)
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
                switch (Server.Version.Type)
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
                    default:
                        return null;
                }
                bi3.EndInit();
                return bi3;
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

        public bool ServerRunning => CurrentStatus == ServerStatus.RUNNING;

        public string AddressInfo { get; set; }

        public string CPUValue => Math.Round(cpuValue,0) + "%";
        public double CPUValueRaw => cpuValue;

        public string MemValue => Math.Round(memValue/Server.JavaSettings.MaxRam *100,0) + "%";
        public double MemValueRaw => memValue/Server.JavaSettings.MaxRam *100;

        public string DiskValue => Math.Round(diskValue, 0) + "%";
        public double DiskValueRaw => diskValue;
        
        public Page ServerPage { get; set; }
        public Page ConsolePage { get; set; }
        public Page WorldsPage { get; set; }
        public SettingsViewModel SettingsViewModel { get; set; }

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

        public double DownloadProgress { get; set; }
        public bool DownloadCompleted { get; set; }
        public double CopyProgress { get; set; }
        public bool ImportCompleted { get; set; } = true;

        public bool ReadyToUse => Server.Initialized && ImportCompleted;

        public ServerViewModel(Server server)
        {
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.ToLower().Contains("isop") || args.PropertyName.ToLower().Contains("playerlist"))
                {
                    Console.WriteLine("Property " + args.PropertyName + " changed");
                }
            };
            DateTime start = DateTime.Now;
            Console.WriteLine("Starting initialization of ViewModel for Server "+server.Name);
            Server = server;
            CurrentStatus = ServerStatus.STOPPED;
            ConsoleOutList = new ObservableCollection<string>();
            new Thread(() =>
            {
                if (Server.Version.Type == ServerVersion.VersionType.Vanilla)
                {
                    Versions = VersionManager.Instance.VanillaVersions;
                }
                else if (Server.Version.Type == ServerVersion.VersionType.Paper)
                {
                    Versions = VersionManager.Instance.PaperVersions;
                }
                else if (Server.Version.Type == ServerVersion.VersionType.Spigot)
                {
                    Versions = VersionManager.Instance.SpigotVersions;
                } 
            }).Start();
            
            ConsoleOutList.CollectionChanged += ConsoleOutChanged;
            UpdateAddressInfo();
            Application.Current.Dispatcher.Invoke(new Action(() => ServerPage = new ServerPage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => ConsolePage = new ConsolePage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => WorldsPage = new WorldsPage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => SettingsViewModel = new SettingsViewModel(this)));

            PlayerList.CollectionChanged += PlayerListChanged;
            WhiteList.CollectionChanged += WhiteListChanged;
            BanList.CollectionChanged += BanListChanged;
            OPList.CollectionChanged += OPListChanged;
            
            Worlds = new ObservableCollection<World>();
            Worlds.CollectionChanged += WorldsChanged;
            
            new Thread(InitializeWorldsList).Start();
            
            new Thread(() =>
            {
                RoleUpdater.InitializeList(RoleType.WHITELIST, WhiteList, Server);
                RoleUpdater.InitializeList(RoleType.BAN_LIST, BanList, Server);
                RoleUpdater.InitializeList(RoleType.OP_LIST, OPList, Server);
                Console.WriteLine("Finished reading Role-lists for "+server);
                PlayerList = new ObservableCollection<ServerPlayer>(PlayerManager.Instance.GetInitialPlayerList(this));
                RefreshPlayerList();
                Console.WriteLine("Initialized PlayerList for server "+server);

                whitelistUpdater = new RoleUpdater(RoleType.WHITELIST, WhiteList,Server.Version);
                banlistUpdater = new RoleUpdater(RoleType.BAN_LIST, BanList,Server.Version);
                oplistUpdater = new RoleUpdater(RoleType.OP_LIST, OPList,Server.Version);
            }).Start();

            TimeSpan t = DateTime.Now - start;
            Console.WriteLine("Server ViewModel for " + server + " initialized in "+t.Seconds+"."+t.Milliseconds+"s");
        }

        private void UpdateAddressInfo()
        {
            AddressInfo = externalIP + ":" + Server.ServerSettings.ServerPort;
        }

        public void RoleInputHandler(string line)
        {
            new Thread(() =>
            {
                whitelistUpdater.HandleOutputLine(line);
                banlistUpdater.HandleOutputLine(line);
                oplistUpdater.HandleOutputLine(line);
            }).Start();
        }

        public void SetRestartTime(double time)
        {
            if (time < 0)
            {
                restartTimer?.Dispose();
                RestartEnabled = false;
                return;
            }

            TimeSpan timeSpan = TimeSpan.FromMilliseconds(time);
            new Thread(() =>
            {
                restartTimer = new System.Timers.Timer();
                restartTimer.Interval = 1000;
                restartTimer.Elapsed += (sender, args) =>
                {
                    timeSpan = timeSpan.Subtract(TimeSpan.FromMilliseconds(1000));
                    RestartEnabled = true;
                    NextRestartHours = timeSpan.Hours.ToString();
                    NextRestartMinutes = timeSpan.Minutes.ToString();
                    NextRestartSeconds = timeSpan.Seconds.ToString();
                    if (timeSpan.Hours == 0 && timeSpan.Minutes == 30 && timeSpan.Seconds == 0)
                    {
                        ApplicationManager.Instance.ActiveServers[Server].StandardInput
                            .WriteLineAsync("/say Next server restart in 30 minutes!");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 5 && timeSpan.Seconds == 0)
                    {
                        ApplicationManager.Instance.ActiveServers[Server].StandardInput
                            .WriteLineAsync("/say Next server restart in 5 minutes!");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 1 && timeSpan.Seconds == 0)
                    {
                        ApplicationManager.Instance.ActiveServers[Server].StandardInput
                            .WriteLineAsync("/say Next server restart in 1 minute!");
                    }
                };
                restartTimer.AutoReset = true;
                restartTimer.Enabled = true;
            }).Start();
        }

        public void UpdateSettings()
        {
            UpdateAddressInfo();
            new Thread(() =>
            {
                new FileWriter().WriteServerSettings(Path.Combine(App.ApplicationPath,Server.Name), Server.ServerSettings.SettingsDictionary);
                Serializer.Instance.StoreServers(ServerManager.Instance.Servers);
            }).Start();
        }

        public void TrackPerformance(Process p)
        {
            // Track CPU usage
            cpuTracker?.StopThreads();

            cpuTracker = new CPUTracker();
            cpuTracker.TrackTotal(p, this);


            // Track memory usage
            memTracker?.StopThreads();

            memTracker = new MEMTracker();
            memTracker.TrackP(p, this);
            
            
            // Track disk usage
            diskTracker?.StopThreads();
            
            diskTracker = new DiskTracker();
            diskTracker.TrackTotal(p,this);
        }

        public void UpdateActiveWorld(World world)
        {
            Server.ServerSettings.LevelName = world.Name;
            UpdateSettings();
        }

        public void CPUValueUpdate(double value)
        {
            cpuValue = value;
            raisePropertyChanged(nameof(CPUValue));
            raisePropertyChanged(nameof(CPUValueRaw));
        }

        public void MemValueUpdate(double value)
        {
            memValue = value;
            raisePropertyChanged(nameof(MemValue));
            raisePropertyChanged(nameof(MemValueRaw));
        }

        public void DiskValueUpdate(double value)
        {
            diskValue = value;
            raisePropertyChanged(nameof(DiskValue));
            raisePropertyChanged(nameof(DiskValueRaw));
        }

        public void StartDownload()
        {
            DownloadCompleted = false;
            Server.Initialized = false;
            Serializer.Instance.StoreServers(ServerManager.Instance.Servers);
            Console.WriteLine("Starting server.jar download for server "+Server.Name);
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
            Server.Initialized = true;
            Serializer.Instance.StoreServers(ServerManager.Instance.Servers);
            Console.WriteLine("Finished downloading server.jar for server " + Server.Name);
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
            raisePropertyChanged(nameof(ImportCompleted));
            raisePropertyChanged(nameof(ReadyToUse));
        }
        
        public void CopyProgressChanged(object sender, FileImporter.CopyProgressChangedEventArgs e)
        {
            CopyProgress = (double)e.FilesCopied / (double)e.FilesToCopy *100;
            //CopyProgressReadable = Math.Round(CopyProgress, 0) + "%";
        }
        
        public void ServerNameChanged()
        {
            raisePropertyChanged(nameof(ServerTitle));
        }
        
        public void InitializeWorldsList()
        {
            DirectoryInfo serverDir = new DirectoryInfo(Path.Combine(App.ApplicationPath,Server.Name));
            if (!serverDir.Exists)
            {
                return;
            }
            Application.Current.Dispatcher?.Invoke(()=>Worlds.Clear());
            foreach (DirectoryInfo directory in serverDir.EnumerateDirectories())
            {
                WorldValidationInfo worldVal = DirectoryValidator.ValidateWorldDirectory(directory);
                if (worldVal.IsValid)
                {
                    World world = new World(worldVal.Name, this, directory);
                    if (Server.ServerSettings.LevelName.Equals(world.Name))
                    {
                        world.IsActive = true;
                    }
                    Application.Current.Dispatcher?.Invoke(()=>Worlds.Add(world));
                }
            }
        }

        public void RefreshPlayerList()
        {
            List<ServerPlayer> players = new List<ServerPlayer>(PlayerList);
            players.Sort();
            PlayerList = new ObservableCollection<ServerPlayer>(players);
            PlayerList.CollectionChanged += PlayerListChanged;
            
            //TODO this is bad WPF (use INotifyPropertyChanged in ServerPlayer instead)
            var c = ConsolePage as ConsolePage;
            Application.Current.Dispatcher?.Invoke(()=>c.Playerlist.Items.Refresh());
        }

        private void ConsoleOutChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(ConsoleOutList));
        }

        private void PlayerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(PlayerList));
        }
        
        private void WhiteListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(WhiteList));
        }
        
        private void WorldsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(Worlds));
        }

        private void BanListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(BanList));
        }

        private void OPListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(OPList));
            EvaluateOPs();
            RefreshPlayerList();
        }

        private void EvaluateOPs()
        {
            foreach (ServerPlayer serverPlayer in PlayerList)
            {
                bool oldOP = serverPlayer.IsOP;
                serverPlayer.IsOP = OPList.Contains(serverPlayer.Player);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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