using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Fork.Logic;
using Fork.Logic.Controller;
using Fork.Logic.ImportLogic;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.Automation;
using Fork.Logic.Persistence;
using Fork.Logic.RoleManagement;
using Fork.View.Xaml2.Pages.Server;
using Console = System.Console;
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

        public Server Server => Entity as Server;

        public bool Initialized { get; set; } = false;

        public string NextAutomationName { get; set; }
        public bool AutomationEnabled { get; set; }
        public string NextAutomationHours { get; set; }
        public string NextAutomationMinutes { get; set; }
        public string NextAutomationSeconds { get; set; }

        public string ServerTitle => Name + " - " + Server.Version.Type + " " + Server.Version.Version;

        public Page WorldsPage { get; set; }

        
        public ServerViewModel(Server server) : base(server)
        {
            if (Server == null)
            {
                throw new Exception();
            }

            DateTime start = DateTime.Now;
            Console.WriteLine("Starting initialization of ViewModel for Server " + Server.Name);
            if (Server.Version.Type == ServerVersion.VersionType.Vanilla)
            {
                Versions = VersionManager.Instance.VanillaVersions;
            }
            else if (Server.Version.Type == ServerVersion.VersionType.Snapshot)
            {
                Versions = VersionManager.Instance.SnapshotVersions;
            }
            else if (Server.Version.Type == ServerVersion.VersionType.Paper)
            {
                Versions = VersionManager.Instance.PaperVersions;
            }
            else if (Server.Version.Type == ServerVersion.VersionType.Purpur)
            {
                Versions = VersionManager.Instance.PurpurVersions;
            } 
            else if (Server.Version.Type == ServerVersion.VersionType.Fabric)
            {
                Versions = VersionManager.Instance.FabricVersions;
            }

            Application.Current.Dispatcher.Invoke(new Action(() => EntityPage = new ServerPage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => ConsolePage = new ConsolePage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => WorldsPage = new WorldsPage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() =>
                PluginsPage = new PluginsPage(new PluginViewModel(this))));
            Application.Current.Dispatcher.Invoke(new Action(() => SettingsViewModel = new SettingsViewModel(this)));

            PlayerList.CollectionChanged += PlayerListChanged;
            WhiteList.CollectionChanged += WhiteListChanged;
            BanList.CollectionChanged += BanListChanged;
            OPList.CollectionChanged += OPListChanged;

            Worlds = new ObservableCollection<World>();
            Worlds.CollectionChanged += WorldsChanged;

            InitializeLists(Server);

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
            Console.WriteLine("Server ViewModel for " + Server.Name + " initialized in " + t.Seconds + "." +
                              t.Milliseconds + "s");
        }

        public void InitializeLists(Server server)
        {
            Task.Run(async () =>
            {
                InitializeWorldsList();
                RoleUpdater.InitializeList(RoleType.WHITELIST, WhiteList, Server);
                RoleUpdater.InitializeList(RoleType.BAN_LIST, BanList, Server);
                RoleUpdater.InitializeList(RoleType.OP_LIST, OPList, Server);
                Console.WriteLine("Finished reading Role-lists for " + server);
                PlayerList = new ObservableCollection<ServerPlayer>();
                await foreach (ServerPlayer player in PlayerManager.Instance.GetInitialPlayerList(this))
                {
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        PlayerList.Add(player);
                        Task.Run(RefreshPlayerList);
                    });
                }

                Console.WriteLine("Initialized PlayerList for server " + server);

                whitelistUpdater = new RoleUpdater(RoleType.WHITELIST, WhiteList, Server.Version);
                banlistUpdater = new RoleUpdater(RoleType.BAN_LIST, BanList, Server.Version);
                oplistUpdater = new RoleUpdater(RoleType.OP_LIST, OPList, Server.Version);
                Initialized = true;
            });
        }

        public void RoleInputHandler(string line)
        {
            Task.Run(() =>
            {
                while (!Initialized)
                {
                    Thread.Sleep(500);
                }
                whitelistUpdater.HandleOutputLine(line);
                banlistUpdater.HandleOutputLine(line);
                oplistUpdater.HandleOutputLine(line);
            });
        }

        public void SetAutomationTime(AutomationTime automationTime)
        {
            restartTimer?.Dispose();
            AutomationEnabled = false;
            if (automationTime == null)
            {
                return;
            }

            if (automationTime is RestartTime)
            {
                NextAutomationName = "Restart";
            }
            else if (automationTime is StopTime)
            {
                NextAutomationName = "Shutdown";
            }
            else if (automationTime is StartTime)
            {
                NextAutomationName = "Starting";
            }

            TimeSpan timeSpan =
                TimeSpan.FromMilliseconds(ServerAutomationManager.Instance.CalculateTime(automationTime));
            AutomationEnabled = true;
            NextAutomationHours = timeSpan.Hours.ToString();
            NextAutomationMinutes = timeSpan.Minutes.ToString();
            NextAutomationSeconds = timeSpan.Seconds.ToString();

            new Thread(() =>
            {
                restartTimer = new Timer {Interval = 1000};
                restartTimer.Elapsed += (sender, args) =>
                {
                    timeSpan = timeSpan.Subtract(TimeSpan.FromMilliseconds(1000));
                    AutomationEnabled = true;
                    NextAutomationHours = timeSpan.Hours.ToString();
                    NextAutomationMinutes = timeSpan.Minutes.ToString();
                    NextAutomationSeconds = timeSpan.Seconds.ToString();
                    if (timeSpan.Hours == 0 && timeSpan.Minutes == 30 && timeSpan.Seconds == 0)
                    {
                        WriteAutomationInfo(automationTime, "30 minutes");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 5 && timeSpan.Seconds == 0)
                    {
                        WriteAutomationInfo(automationTime, "5 minutes");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 1 && timeSpan.Seconds == 0)
                    {
                        WriteAutomationInfo(automationTime, "1 minute");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 10)
                    {
                        WriteAutomationInfo(automationTime, "10 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 9)
                    {
                        WriteAutomationInfo(automationTime, "9 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 8)
                    {
                        WriteAutomationInfo(automationTime, "8 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 7)
                    {
                        WriteAutomationInfo(automationTime, "7 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 6)
                    {
                        WriteAutomationInfo(automationTime, "6 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 5)
                    {
                        WriteAutomationInfo(automationTime, "5 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 4)
                    {
                        WriteAutomationInfo(automationTime, "4 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 3)
                    {
                        WriteAutomationInfo(automationTime, "3 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 2)
                    {
                        WriteAutomationInfo(automationTime, "2 seconds");
                    }
                    else if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 1)
                    {
                        WriteAutomationInfo(automationTime, "1 seconds");
                    }
                };
                restartTimer.AutoReset = true;
                restartTimer.Enabled = true;
            }) {IsBackground = true}.Start();
        }

        public async Task SaveProperties()
        {
            if (Server.ServerSettings.HasChanged)
            {
                await new FileWriter().WriteServerSettings(Path.Combine(App.ServerPath, Server.Name),
                    Server.ServerSettings.SettingsDictionary);
                EntitySerializer.Instance.StoreEntities();
                Server.ServerSettings.HasChanged = false;
            }
        }

        public void UpdateActiveWorld(World world)
        {
            Server.ServerSettings.LevelName = world.Name;
            SaveSettings();
        }

        public void ServerNameChanged()
        {
            EntitySerializer.Instance.StoreEntities();
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
            PlayerList.Sort();
        }

        private void WriteAutomationInfo(AutomationTime automationTime, string time)
        {
            if (automationTime is RestartTime)
            {
                ApplicationManager.Instance.ActiveEntities[Server].StandardInput
                    .WriteLineAsync("say Next server restart in " + time + "!");
            }
            else if (automationTime is StopTime)
            {
                ApplicationManager.Instance.ActiveEntities[Server].StandardInput
                    .WriteLineAsync("say Server shutdown in " + time + "!");
            }
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