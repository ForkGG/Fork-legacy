using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Fork.Logic.Controller;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.ProxyModels;
using Fork.Logic.Model.ProxyModels;
using Fork.Logic.Model.Settings;
using Fork.Logic.Persistence;
using Fork.View.Xaml2.Pages.Network;
using Fork.View.Xaml2.Pages.Server;
using Fork.View.Xaml2.Pages.Settings;
using GongSolutions.Wpf.DragDrop;
using ConsolePage = Fork.View.Xaml2.Pages.Network.ConsolePage;
using Server = Fork.Logic.Model.Server;

namespace Fork.ViewModel
{
    public class NetworkViewModel : EntityViewModel
    {
        public Network Network
        {
            get { return Entity as Network; }
            set { Entity = value; }
        }

        public ObservableCollection<NetworkServer> Servers { get; set; } = new ObservableCollection<NetworkServer>();
        public ObservableCollection<Permission> Permissions { get; set; } = new ObservableCollection<Permission>();
        public ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();

        public string NetworkTitle => Name + " - " + Network.Version.Type;

        public ServerDropHandler DropHandler { get; }

        public NetworkViewModel(Network network) : base(network)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("Starting initialization of ViewModel for Network " + network.Name);
            Network = network;
            TimeSpan t = DateTime.Now - start;
            ReadSettings();
            UpdateAddressInfo();
            Application.Current.Dispatcher.Invoke(new Action(() => EntityPage = new NetworkPage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => ConsolePage = new ConsolePage(this)));
            Application.Current.Dispatcher.Invoke(new Action(() => PluginsPage = new PluginsPage(new PluginViewModel(this))));
            Application.Current.Dispatcher.Invoke(new Action(() => SettingsViewModel = new SettingsViewModel(this)));

            DropHandler = new ServerDropHandler(this);

            if (!ApplicationManager.Initialized)
            {
                ApplicationManager.ApplicationInitialized +=
                    () => Application.Current.Dispatcher?.Invoke(StartSettingsReader);
            }
            else
            {
                Application.Current.Dispatcher?.Invoke(StartSettingsReader);
            }

            Console.WriteLine("Server ViewModel for " + network.Name + " initialized in " + t.Seconds + "." +
                              t.Milliseconds + "s");
        }

        public void RemoveServer(NetworkServer networkServer)
        {
            if (networkServer != null && Servers.Contains(networkServer))
            {
                Application.Current.Dispatcher?.Invoke(() => Servers.Remove(networkServer));
                SaveSettings();
            }
        }

        public void AddServer(ServerViewModel serverViewModel, int index)
        {
            string uid = serverViewModel.Server.UID;
            Fork.Logic.Model.Settings.Server server = new Logic.Model.Settings.Server
            {
                address = "0.0.0.0:" + serverViewModel.Server.ServerSettings.ServerPort,
                ForkServer = true,
                ForkServerUid = uid,
                motd = serverViewModel.Server.ServerSettings.Motd,
                restricted = false
            };
            NetworkForkServer networkForkServer =
                new NetworkForkServer(serverViewModel, server, serverViewModel.Server.Name);

            bool contains = false;
            foreach (NetworkServer networkServer in Servers)
            {
                if (networkServer is NetworkForkServer ForkServer)
                {
                    if (ForkServer.ServerViewModel.Server.UID.Equals(uid))
                    {
                        contains = true;
                    }
                }
            }

            if (!contains)
            {
                Application.Current.Dispatcher?.Invoke(() => Servers.Insert(index, networkForkServer));
                SaveSettings();
            }
        }

        public void AddServer(Fork.Logic.Model.Settings.Server server, string name)
        {
            NetworkExternalServer networkExternalServer = new NetworkExternalServer(server, name);
            if (!Network.Config.servers.ContainsKey(name))
            {
                Application.Current.Dispatcher?.Invoke(() => Servers.Add(networkExternalServer));
                SaveSettings();
            }
        }

        public void RemovePermission(Permission permission)
        {
            if (Permissions.Contains(permission))
            {
                Application.Current.Dispatcher?.Invoke(() => Permissions.Remove(permission));
                SaveSettings();
            }
        }

        public void AddPermission(Permission permission)
        {
            foreach (Permission p in Permissions)
            {
                if (p.Name.Equals(permission.Name))
                {
                    return;
                }
            }

            Application.Current.Dispatcher?.Invoke(() => Permissions.Add(permission));
            SaveSettings();
        }

        public void RemoveGroup(Group group)
        {
            if (Groups.Contains(group))
            {
                Application.Current.Dispatcher?.Invoke(() => Groups.Remove(group));
                SaveSettings();
            }
        }

        public void AddGroup(Group group)
        {
            foreach (Group g in Groups)
            {
                if (g.User.Equals(group.User))
                {
                    return;
                }
            }

            Application.Current.Dispatcher?.Invoke(() => Groups.Add(group));
            SaveSettings();
        }

        public void ReadSettings()
        {
            Network.ReadSettings();
            new Thread(() =>
            {
                LoadGroupsFromConfig(Network.Config);
                LoadPermissionsFromConfig(Network.Config);
                LoadServersFromConfig(Network.Config);
            }).Start();
        }
        
        public void SaveConfig()
        {
            new Thread(() =>
            {
                StoreServersToConfig(Network.Config);
                StorePermissionsToConfig(Network.Config);
                StoreGroupsToConfig(Network.Config);
                Network.WriteSettings();
                UpdateAddressInfo();
            }).Start();
        }

        public void UpdateServer(NetworkServer networkServer, ServerViewModel serverViewModel)
        {
            if (Servers.Contains(networkServer) 
                && networkServer is NetworkForkServer networkForkServer 
                && networkForkServer.ServerViewModel == serverViewModel)
            {
                networkServer.Address = "0.0.0.0:" + serverViewModel.Server.ServerSettings.ServerPort;
                networkServer.Motd = serverViewModel.Server.ServerSettings.Motd;
                SaveConfig();
                //Update UI
                foreach (ISettingsPage page in SettingsViewModel.SettingsPages)
                {
                    if (page is ProxySettingsPage settingsPage)
                    {
                        Application.Current.Dispatcher?.Invoke(() => settingsPage.Reload());
                    }
                }
            }
            
        }

        private void UpdateAddressInfo()
        {
            AddressInfo = new APIController().GetExternalIPAddress() + ":" + Network.Port;
        }

        private void LoadServersFromConfig(BungeeSettings settings)
        {
            while (!ServerManager.Initialized)
            {
                Thread.Sleep(500);
            }

            List<NetworkServer> servers = new List<NetworkServer>();
            if (settings.servers == null)
            {
                Servers = new ObservableCollection<NetworkServer>();
                return;
            }

            foreach (var settingsServer in settings.servers)
            {
                string name = settingsServer.Key;
                if (settingsServer.Value.ForkServer)
                {
                    EntityViewModel viewModel =
                        ServerManager.Instance.GetEntityViewModelByUid(settingsServer.Value.ForkServerUid);
                    if (viewModel is ServerViewModel serverViewModel)
                    {
                        NetworkForkServer server = new NetworkForkServer(serverViewModel, settingsServer.Value, name);
                        servers.Add(server);
                    }
                    else
                    {
                        Console.WriteLine("[FATAL] Error finding server with UID " +
                                          settingsServer.Value.ForkServerUid + " for network " + Network.Name);
                        ErrorLogger.Append(new Exception("[FATAL] Error finding server with UID " +
                                                         settingsServer.Value.ForkServerUid + " for network " +
                                                         Network.Name));
                    }
                }
                else
                {
                    NetworkExternalServer server = new NetworkExternalServer(settingsServer.Value, name);
                    servers.Add(server);
                }
            }

            Servers = new ObservableCollection<NetworkServer>(servers);
        }

        private void StoreServersToConfig(BungeeSettings settings)
        {
            settings.servers = new Dictionary<string, Logic.Model.Settings.Server>();
            foreach (NetworkServer networkServer in Servers)
            {
                settings.servers.Add(networkServer.Name, networkServer.ProxyServer);
            }

            if (settings.listeners.Count < 1)
            {
                settings.listeners.Add(new Listener());
            }

            settings.listeners[0].priorities = new List<string>();
            foreach (NetworkServer server in Servers)
            {
                settings.listeners[0].priorities.Add(server.Name);
            }
        }

        private void LoadPermissionsFromConfig(BungeeSettings settings)
        {
            while (!ServerManager.Initialized)
            {
                Thread.Sleep(500);
            }

            List<Permission> permissions = new List<Permission>();
            if (settings.permissions == null)
            {
                Permissions = new ObservableCollection<Permission>();
                return;
            }

            foreach (KeyValuePair<string, List<string>> keyValuePair in settings.permissions)
            {
                Permission p = new Permission(keyValuePair.Key, new ObservableCollection<string>(keyValuePair.Value));
                permissions.Add(p);
            }

            Permissions = new ObservableCollection<Permission>(permissions);
        }

        private void StorePermissionsToConfig(BungeeSettings settings)
        {
            settings.permissions = new Dictionary<string, List<string>>();
            foreach (Permission permission in Permissions)
            {
                settings.permissions.Add(permission.Name, new List<string>(permission.PermissionList));
            }
        }

        private void LoadGroupsFromConfig(BungeeSettings settings)
        {
            while (!ServerManager.Initialized)
            {
                Thread.Sleep(500);
            }

            List<Group> groups = new List<Group>();
            if (settings.groups == null)
            {
                Groups = new ObservableCollection<Group>();
                return;
            }

            foreach (KeyValuePair<string, List<string>> keyValuePair in settings.groups)
            {
                Group g = new Group(keyValuePair.Key, new ObservableCollection<string>(keyValuePair.Value));
                groups.Add(g);
            }

            Groups = new ObservableCollection<Group>(groups);
        }

        private void StoreGroupsToConfig(BungeeSettings settings)
        {
            settings.groups = new Dictionary<string, List<string>>();
            foreach (Group group in Groups)
            {
                settings.groups.Add(group.User, new List<string>(group.Groups));
            }
        }

        public class ServerDropHandler : IDropTarget
        {
            private NetworkViewModel networkViewModel;
            private DefaultDropHandler dropHandler = new DefaultDropHandler();

            public ServerDropHandler(NetworkViewModel viewModel)
            {
                networkViewModel = viewModel;
            }

            public void DragOver(IDropInfo dropInfo)
            {
                if (dropInfo.Data is NetworkServer && dropInfo.TargetItem is NetworkServer)
                {
                    dropHandler.DragOver(dropInfo);
                }
                else if (dropInfo.Data is ServerViewModel)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }

            public void Drop(IDropInfo dropInfo)
            {
                if (dropInfo.Data is ServerViewModel serverViewModel)
                {
                    networkViewModel.AddServer(serverViewModel, dropInfo.InsertIndex);
                }

                if (dropInfo.Data is NetworkServer)
                {
                    dropHandler.Drop(dropInfo);
                }
            }
        }
    }
}