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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fork.Annotations;
using Fork.Logic.BackgroundWorker.Performance;
using Fork.Logic.Controller;
using Fork.Logic.CustomConsole;
using Fork.Logic.ImportLogic;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Persistence;
using Fork.Logic.RoleManagement;
using Fork.View.Xaml2.Pages;
using Fork.View.Xaml2.Pages;
using Fork.View.Xaml2.Pages.Server;
using Fork.View.Xaml2.Pages.Server;
using Console = System.Console;
using RadioButton = System.Windows.Forms.RadioButton;
using Timer = System.Timers.Timer;

namespace Fork.ViewModel
{
    public class ServerViewModel : EntityViewModel
    {
        private Timer restartTimer = null;

        private RoleUpdater whitelistUpdater;
        private RoleUpdater banlistUpdater;
        private RoleUpdater oplistUpdater;
        private World activeWorld;


        public ObservableCollection<ServerPlayer> PlayerList { get; set; } = new ObservableCollection<ServerPlayer>();
        public ObservableCollection<Player> BanList { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<Player> OPList { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<Player> WhiteList { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<ServerVersion> Versions { get; set; } = new ObservableCollection<ServerVersion>();
        public ObservableCollection<World> Worlds { get; set; }

        public Server Server
        {
            get => Entity as Server;
            set => Entity = value;
        }

        public bool RestartEnabled { get; set; }
        public string NextRestartHours { get; set; }
        public string NextRestartMinutes { get; set; }
        public string NextRestartSeconds { get; set; }

        public string ServerTitle => Name + " - " + Server.Version.Type + " " + Server.Version.Version;

        public Page WorldsPage { get; set; }
        
        public Page PluginsPage { get; set; }

        public ServerViewModel(Server server) : base(server)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("Starting initialization of ViewModel for Server " + server.Name);
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

            UpdateAddressInfo();
            Application.Current.Dispatcher.Invoke(new Action(() => EntityPage = new ServerPage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => ConsolePage = new ConsolePage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => WorldsPage = new WorldsPage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => PluginsPage = new PluginsPage(new PluginViewModel(this))));
            Application.Current.Dispatcher.Invoke(new Action(() => SettingsViewModel = new SettingsViewModel(this)));

            PlayerList.CollectionChanged += PlayerListChanged;
            WhiteList.CollectionChanged += WhiteListChanged;
            BanList.CollectionChanged += BanListChanged;
            OPList.CollectionChanged += OPListChanged;

            Worlds = new ObservableCollection<World>();
            Worlds.CollectionChanged += WorldsChanged;

            InitializeLists(server);

            if (!ApplicationManager.Initialized)
            {
                ApplicationManager.ApplicationInitialized +=
                    () => Application.Current.Dispatcher?.Invoke(StartSettingsReader);
            }
            else
            {
                Application.Current.Dispatcher?.Invoke(StartSettingsReader);
            }

            TimeSpan t = DateTime.Now - start;
            Console.WriteLine("Server ViewModel for " + server.Name + " initialized in " + t.Seconds + "." +
                              t.Milliseconds + "s");
        }

        public void InitializeLists(Server server)
        {
            new Thread(() =>
            {
                InitializeWorldsList();
                RoleUpdater.InitializeList(RoleType.WHITELIST, WhiteList, Server);
                RoleUpdater.InitializeList(RoleType.BAN_LIST, BanList, Server);
                RoleUpdater.InitializeList(RoleType.OP_LIST, OPList, Server);
                Console.WriteLine("Finished reading Role-lists for " + server);
                PlayerList = new ObservableCollection<ServerPlayer>(PlayerManager.Instance.GetInitialPlayerList(this));
                RefreshPlayerList();
                Console.WriteLine("Initialized PlayerList for server " + server);

                whitelistUpdater = new RoleUpdater(RoleType.WHITELIST, WhiteList, Server.Version);
                banlistUpdater = new RoleUpdater(RoleType.BAN_LIST, BanList, Server.Version);
                oplistUpdater = new RoleUpdater(RoleType.OP_LIST, OPList, Server.Version);
            }).Start();
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
                        ApplicationManager.Instance.ActiveEntities[Server].StandardInput
                            .WriteLineAsync("/say Next server restart in 30 minutes!");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 5 && timeSpan.Seconds == 0)
                    {
                        ApplicationManager.Instance.ActiveEntities[Server].StandardInput
                            .WriteLineAsync("/say Next server restart in 5 minutes!");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 1 && timeSpan.Seconds == 0)
                    {
                        ApplicationManager.Instance.ActiveEntities[Server].StandardInput
                            .WriteLineAsync("/say Next server restart in 1 minute!");
                    }
                };
                restartTimer.AutoReset = true;
                restartTimer.Enabled = true;
            }).Start();
        }

        private void UpdateAddressInfo()
        {
            AddressInfo = new APIController().GetExternalIPAddress() + ":" + Server.ServerSettings.ServerPort;
        }

        public void UpdateSettings()
        {
            new Thread(() =>
            {
                UpdateAddressInfo();
                SettingsViewModel.SaveChanges();
                EntitySerializer.Instance.StoreEntities(ServerManager.Instance.Entities);
            }).Start();
        }

        public void SaveProperties()
        {
            new Thread(() =>
            {
                new FileWriter().WriteServerSettings(Path.Combine(App.ServerPath, Server.Name),
                    Server.ServerSettings.SettingsDictionary);
            }).Start();
        }

        public void UpdateActiveWorld(World world)
        {
            Server.ServerSettings.LevelName = world.Name;
            UpdateSettings();
        }

        public void ServerNameChanged()
        {
            raisePropertyChanged(nameof(ServerTitle));
        }

        public void InitializeWorldsList()
        {
            DirectoryInfo serverDir = new DirectoryInfo(Path.Combine(App.ServerPath, Server.Name));
            if (!serverDir.Exists)
            {
                return;
            }

            Application.Current.Dispatcher?.Invoke(() => Worlds.Clear());
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

                    Application.Current.Dispatcher?.Invoke(() => Worlds.Add(world));
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
            Application.Current.Dispatcher?.Invoke(() => c.Playerlist.Items.Refresh());
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
    }
}