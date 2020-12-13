using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Fork.Logic.BackgroundWorker;
using Fork.Logic.Controller;
using Fork.Logic.CustomConsole;
using Fork.Logic.ImportLogic;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.logic.model.PluginModels;
using Fork.Logic.Persistence;
using Fork.Logic.WebRequesters;
using Fork.ViewModel;
using Newtonsoft.Json;

namespace Fork.Logic.Manager
{
    public sealed class ServerManager
    {
        private static ServerManager instance = null;

        private ServerManager()
        {
            if (new DirectoryInfo(Path.Combine(App.ApplicationPath, "persistence")).Exists)
            {
                entities = EntitySerializer.Instance.LoadEntities();
            }

            if (entities == null)
            {
                entities = new ObservableCollection<EntityViewModel>();
            }

            foreach (EntityViewModel viewModel in Entities)
            {
                if (!viewModel.Entity.Initialized)
                {
                    Downloader.DownloadJarAsync(viewModel, new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Name)));
                }
            }

            Entities.CollectionChanged += ServerListChanged;

            serverNames = new List<string>();
            foreach (EntityViewModel server in Entities)
            {
                serverNames.Add(server.Entity.Name);
            }
            initialized = true;
        }

        public static ServerManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ServerManager();
                return instance;
            }
        }

        private static bool initialized = false;

        public static bool Initialized => initialized;

        private List<string> serverNames;
        private NetworkController networkController = new NetworkController();

        private ObservableCollection<EntityViewModel> entities;

        public ObservableCollection<EntityViewModel> Entities => 
            entities ??= new ObservableCollection<EntityViewModel>();

        public async Task<bool> MoveEntitiesAsync(string newPath)
        {
            Task<bool> t = new Task<bool>(() => MoveEntities(newPath));
            t.Start();
            bool r = await t;
            return r;
        }
        
        #region Server Managment Methods
        public async Task<bool> CreateServerAsync(string serverName, ServerVersion serverVersion, ServerSettings serverSettings,
            JavaSettings javaSettings, string worldPath = null)
        {
            Task<bool> t = new Task<bool>(() =>
                CreateServer(serverName, serverVersion, serverSettings, javaSettings, worldPath));
            t.Start();
            bool r = await t;
            return r;
        }
        
        public async Task<bool> ImportServerAsync(ServerVersion version, ServerValidationInfo validationInfo,
            string originalServerDirectory, string serverName)
        {
            Task<bool> t = new Task<bool>(() =>
                ImportServer(version, validationInfo, originalServerDirectory, serverName));
            t.Start();
            return await t;
        }
        
        public void StopServer(ServerViewModel serverViewModel)
        {
            if (!ApplicationManager.Instance.ActiveEntities.ContainsKey(serverViewModel.Server))
            {
                Console.WriteLine("Can't stop server that has no active process");
                return;
            }
            ApplicationManager.Instance.ActiveEntities[serverViewModel.Server].StandardInput.WriteLine("stop");
            foreach (ServerPlayer serverPlayer in serverViewModel.PlayerList)
            {
                serverPlayer.IsOnline = false;
            }
            serverViewModel.RefreshPlayerList();
        }
        
        public async Task<bool> StartServerAsync(ServerViewModel serverViewModel)
        {
            Task<bool> t = new Task<bool>(() => StartServer(serverViewModel));
            t.Start();
            bool result = await t;
            return result;
        }
        
        public async Task<bool> RestartServerAsync(ServerViewModel serverViewModel)
        {
            Task<bool> t = new Task<bool>(() => RestartServer(serverViewModel));
            t.Start();
            bool result = await t;
            return result;
        }
        
        public async Task<bool> RenameServerAsync(ServerViewModel viewModel, string newName)
        {
            Task<bool> t = new Task<bool>(() => RenameServer(viewModel, newName));
            t.Start();
            bool r = await t;
            return r;
        }

        public async Task<bool> CloneServerAsync(ServerViewModel viewModel)
        {
            Task<bool> t = new Task<bool>(() => CloneServer(viewModel));
            t.Start();
            bool r = await t;
            return r;
        }

        public async Task<bool> DeleteServerAsync(ServerViewModel serverViewModel)
        {
            if (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
            {
                StopServer(serverViewModel);
                while (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
                {
                    Thread.Sleep(500);
                }
            }
            Task<bool> t = new Task<bool>(() => DeleteServer(serverViewModel));
            t.Start();
            bool result = await t;
            return result;
        }

        public async Task<bool> ChangeServerVersionAsync(ServerVersion newVersion, ServerViewModel serverViewModel)
        {
            Task<bool> t = new Task<bool>(() => ChangeServerVersion(newVersion, serverViewModel));
            t.Start();
            bool result = await t;
            return result;
        }
        
        public bool RestartServer(ServerViewModel serverViewModel)
        {
            StopServer(serverViewModel);
            while (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
            {
                Thread.Sleep(500);
            }

            StartServer(serverViewModel);

            return true;
        }

        #endregion

        #region World Managment Methods

        public async Task<bool> ImportWorldAsync(ServerViewModel viewModel, string worldSource)
        {
            Task<bool> t = new Task<bool>(()=>
                ImportWorld(viewModel, worldSource));
            t.Start();
            return await t;
        }
        
        public async Task<bool> CreateWorldAsync(string name, ServerViewModel viewModel)
        {
            Task<bool> t = new Task<bool>(()=>
                CreateWorld(name, viewModel));
            t.Start();
            return await t;
        }
        
        public async Task<bool> DeleteDimensionAsync(MinecraftDimension dimension, Server server)
        {
            Task<bool> t = new Task<bool>(() => DeleteDimension(dimension, server));
            t.Start();
            bool result = await t;
            return result;
        }

        #endregion

        #region Network Managment Methods

        public async Task<bool> CreateNetworkAsync(string networkName, ServerVersion.VersionType networkType, JavaSettings javaSettings)
        {
            Task<bool> t = new Task<bool>(() =>
                networkController.CreateNetwork(networkName,networkType, javaSettings, serverNames));
            t.Start();
            bool r = await t;
            return r;
        }

        public async Task<bool> StartNetworkAsync(NetworkViewModel viewModel, bool startServers = false)
        {
            Task<bool> t = new Task<bool>(() =>
                networkController.StartNetwork(viewModel, startServers));
            t.Start();
            bool r = await t;
            return r;
        }

        public async Task<bool> StopNetworkAsync(NetworkViewModel viewModel, bool stopServers = false)
        {
            Task<bool> t = new Task<bool>(() =>
                networkController.StopNetwork(viewModel, stopServers));
            t.Start();
            bool r = await t;
            return r;
        }

        public async Task<bool> RestartNetworkAsync(NetworkViewModel viewModel, bool restartServers = false)
        {
            bool stopSuccess = await StopNetworkAsync(viewModel, restartServers);
            if (!stopSuccess)
            {
                return false;
            }
            bool startSuccess = await StartNetworkAsync(viewModel, restartServers);
            return startSuccess;
        }

        public async Task<bool> RenameNetworkAsync(NetworkViewModel viewModel, string newName)
        {
            Task<bool> t = new Task<bool>(() =>
                networkController.RenameNetwork(viewModel, newName));
            t.Start();
            bool r = await t;
            return r;
        }
        
        public async Task<bool> CloneNetworkAsync(NetworkViewModel viewModel)
        {
            Task<bool> t = new Task<bool>(() =>
                networkController.CloneNetwork(viewModel, serverNames));
            t.Start();
            bool r = await t;
            return r;
        }
        
        public async Task<bool> DeleteNetworkAsync(NetworkViewModel viewModel)
        {
            Task<bool> t = new Task<bool>(() =>
                networkController.DeleteNetwork(viewModel));
            t.Start();
            bool r = await t;
            return r;
        }

        public async Task<bool> KillNetworkAsync(NetworkViewModel viewModel, bool killServers = false)
        {
            Task<bool> t = new Task<bool>(() =>
                networkController.KillNetwork(viewModel, killServers));
            t.Start();
            bool r = await t;
            return r;
        }

        #endregion

        public void AddEntity(EntityViewModel entityViewModel)
        {
            serverNames.Add(entityViewModel.Entity.Name);
            Application.Current.Dispatcher.Invoke(() => Entities.Add(entityViewModel));
        }

        public void RemoveEntity(EntityViewModel entityViewModel)
        {
            serverNames.Remove(entityViewModel.Entity.Name);
            Application.Current.Dispatcher.Invoke(() => Entities.Remove(entityViewModel));
        }

        public EntityViewModel GetEntityViewModelByUid(string uid)
        {
            return Entities.FirstOrDefault(entityViewModel => entityViewModel.Entity.UID.Equals(uid));
        }

        private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EntitySerializer.Instance.StoreEntities(Entities);
            ApplicationManager.Instance.MainViewModel.SetServerList(ref entities);
        }

        private bool MoveEntities(string newPath)
        {
            DirectoryInfo newDir = new DirectoryInfo(newPath);
            if (!newDir.Exists)
            {
                ErrorLogger.Append(new Exception("Can't move Servers/Networks to not existing dir: "+newPath));
                return false;
            }

            foreach (EntityViewModel entityViewModel in Entities)
            {
                string currEntityPath = Path.Combine(App.ServerPath, entityViewModel.Entity.Name);
                string newEntityPath = Path.Combine(newPath, entityViewModel.Entity.Name);
                
                entityViewModel.StartImport();
                new Thread(() =>
                {
                    //Directory.Move(currEntityPath,newEntityPath);
                    FileImporter fileImporter = new FileImporter();
                    fileImporter.CopyProgressChanged += entityViewModel.CopyProgressChanged;
                    fileImporter.DirectoryMove(currEntityPath, newEntityPath, true);
                    Console.WriteLine("Finished moving entity files for entity "+entityViewModel.Name);
                    entityViewModel.FinishedCopying();
                }).Start();
            }

            AppSettingsSerializer.Instance.AppSettings.ServerPath = newPath;
            AppSettingsSerializer.Instance.SaveSettings();
            return true;
        }

        private bool ImportServer(ServerVersion version, ServerValidationInfo validationInfo,
            string originalServerDirectory, string serverName)
        {
            string serverPath = Path.Combine(App.ServerPath, serverName);
            while (Directory.Exists(serverPath))
            {
                serverPath += "-Copy";
                serverName += "-Copy";
            }
            
            ServerSettings settings;
            if (new FileInfo(Path.Combine(originalServerDirectory, "server.properties")).Exists)
            {
                var settingsDict = new FileReader().ReadServerSettings(originalServerDirectory);
                settings = new ServerSettings(settingsDict);
            }
            else
            {
                string worldName = validationInfo.Worlds.First().Name;
                settings = new ServerSettings(worldName);
            }

            Server server = new Server(serverName, version, settings, new JavaSettings());
            serverNames.Add(serverName);
            
            //Add server to Fork
            ServerViewModel viewModel = new ServerViewModel(server);
            viewModel.StartImport();
            Application.Current.Dispatcher.Invoke(() => Entities.Add(viewModel));
            ApplicationManager.Instance.MainViewModel.SelectedEntity = viewModel;
            
            //Create server directory
            DirectoryInfo serverDirectory = Directory.CreateDirectory(serverPath);
            
            //Import server files
            Thread copyThread = new Thread(() =>
            {
                FileImporter fileImporter = new FileImporter();
                fileImporter.CopyProgressChanged += viewModel.CopyProgressChanged;
                fileImporter.DirectoryCopy(originalServerDirectory, serverPath, true, new List<string>{"server.jar"});
                Console.WriteLine("Finished copying server files for server "+serverName);
                viewModel.FinishedCopying();
            });
            copyThread.Start();
            
            //Download server.jar
            Thread thread = new Thread(() =>
            {
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += viewModel.DownloadProgressChanged;
                webClient.DownloadFileCompleted += viewModel.DownloadCompletedHandler;
                webClient.DownloadFileAsync(new Uri(server.Version.JarLink),
                    Path.Combine(serverDirectory.FullName, "server.jar"));
            }){IsBackground = true};
            thread.Start();
            
            if (!validationInfo.EulaTxt)
            {
                //Write Eula
                new FileWriter().WriteEula(serverPath);
            }
            
            
            return new DirectoryInfo(serverPath).Exists;
        }

        private bool ImportWorld(ServerViewModel viewModel, string worldSource)
        {
            try
            {
                DirectoryInfo serverDir = new DirectoryInfo(Path.Combine(App.ServerPath,viewModel.Server.Name));
                DirectoryInfo importWorldDir = new DirectoryInfo(worldSource);
                string worldName = importWorldDir.Name;
                List<string> worlds = new List<string>();
                foreach (World world in viewModel.Worlds)
                {
                    worlds.Add(world.Name);
                }
                while (worlds.Contains(worldName))
                {
                    worldName += "1";
                }
                if (!serverDir.Exists||!importWorldDir.Exists)
                {
                    Console.WriteLine("Error during world import! Server or World directory don't exist");
                    return false;
                }
                new FileImporter().DirectoryCopy(importWorldDir.FullName,
                    Path.Combine(serverDir.FullName, worldName), true);
                viewModel.InitializeWorldsList();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        private bool CreateWorld(string worldName, ServerViewModel viewModel)
        {
            try
            {
                DirectoryInfo serverDir = new DirectoryInfo(Path.Combine(App.ServerPath,viewModel.Server.Name));
                List<string> worlds = new List<string>();
                foreach (World world in viewModel.Worlds)
                {
                    worlds.Add(world.Name);
                }
                while (worlds.Contains(worldName))
                {
                    worldName += "1";
                }
                if (!serverDir.Exists)
                {
                    Console.WriteLine("Error during world import! Server or World directory don't exist");
                    return false;
                }
                DirectoryInfo worldDir = Directory.CreateDirectory(Path.Combine(serverDir.FullName, worldName));
                Directory.CreateDirectory(Path.Combine(worldDir.FullName, "region"));
                Directory.CreateDirectory(Path.Combine(worldDir.FullName, "data"));
                viewModel.InitializeWorldsList();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        private bool CreateServer(string serverName, ServerVersion serverVersion, ServerSettings serverSettings,
            JavaSettings javaSettings, string worldPath = null)
        {
            serverName = RefineName(serverName);
            string serverPath = Path.Combine(App.ServerPath, serverName);
            serverNames.Add(serverName);
            if (string.IsNullOrEmpty(serverSettings.LevelName))
            {
                serverSettings.LevelName = "world";
            }
            DirectoryInfo directoryInfo = Directory.CreateDirectory(serverPath);
            Server server = new Server(serverName, serverVersion, serverSettings, javaSettings);
            ServerViewModel viewModel = new ServerViewModel(server);
            Application.Current.Dispatcher.Invoke(() => Entities.Add(viewModel));
            //Select Server
            ApplicationManager.Instance.MainViewModel.SelectedEntity = viewModel;
            
            //Download server.jar
            Downloader.DownloadJarAsync(viewModel,directoryInfo);
            
            //Move World Files
            if (worldPath != null)
            {
                new FileImporter().DirectoryCopy(worldPath, Path.Combine(directoryInfo.FullName,server.ServerSettings.LevelName), true);
            }
            
            //Writing necessary files
            new FileWriter().WriteEula(Path.Combine(App.ServerPath, directoryInfo.Name));
            new FileWriter().WriteServerSettings(Path.Combine(App.ServerPath, directoryInfo.Name),
                serverSettings.SettingsDictionary);

            return true;
        }
        
        private bool RenameServer(ServerViewModel viewModel, string newName)
        {
            if (viewModel.CurrentStatus != ServerStatus.STOPPED)
            {
                StopServer(viewModel);
                while (viewModel.CurrentStatus != ServerStatus.STOPPED)
                {
                    Thread.Sleep(500);
                }
            }
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Name));
                if (!directoryInfo.Exists)
                {
                    ErrorLogger.Append(
                        new DirectoryNotFoundException("Could not find Directory " + directoryInfo.FullName));
                    return false;
                }

                directoryInfo.MoveTo(Path.Combine(App.ServerPath, newName));

                viewModel.Name = newName;
                return true;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return false;
            }
        }

        private bool CloneServer(ServerViewModel viewModel)
        {
            if (viewModel.CurrentStatus != ServerStatus.STOPPED)
            {
                StopServer(viewModel);
                while (viewModel.CurrentStatus != ServerStatus.STOPPED)
                {
                    Thread.Sleep(500);
                }
            }
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Name));
                if (!directoryInfo.Exists)
                {
                    ErrorLogger.Append(
                        new DirectoryNotFoundException("Could not find Directory " + directoryInfo.FullName));
                    return false;
                }
                string newName = RefineName(viewModel.Name+"-Clone");
                
                //Better to use a object copy function
                string oldServerJson = JsonConvert.SerializeObject(viewModel.Server);
                Server newServer = JsonConvert.DeserializeObject<Server>(oldServerJson);
                
                newServer.Name = newName;
                newServer.UID = Guid.NewGuid().ToString();
                ServerViewModel newServerViewModel = new ServerViewModel(newServer);

                string newServerPath = Path.Combine(App.ServerPath, newName);
                newServerViewModel.StartImport();
                Application.Current.Dispatcher?.Invoke(() => Entities.Add(newServerViewModel));
                ApplicationManager.Instance.MainViewModel.SelectedEntity = newServerViewModel;
            
                //Create server directory
                Directory.CreateDirectory(newServerPath);
            
                //Import server files
                Thread copyThread = new Thread(() =>
                {
                    FileImporter fileImporter = new FileImporter();
                    fileImporter.CopyProgressChanged += newServerViewModel.CopyProgressChanged;
                    fileImporter.DirectoryCopy(directoryInfo.FullName, newServerPath, true, new List<string>{"server.jar"});
                    Console.WriteLine("Finished copying server files for server "+newServerPath);
                    newServerViewModel.FinishedCopying();
                });
                copyThread.Start();
                
                return true;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return false;
            }
        }

        private bool DeleteServer(ServerViewModel serverViewModel)
        {
            try
            {
                if (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
                {
                    StopServer(serverViewModel);
                    while (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
                    {
                        Thread.Sleep(500);
                    }
                }

                DirectoryInfo deletedDirectory =
                    Directory.CreateDirectory(Path.Combine(App.ServerPath, "backup", "deleted"));
                if (File.Exists(Path.Combine(deletedDirectory.FullName, serverViewModel.Name + ".zip")))
                {
                    File.Delete(Path.Combine(deletedDirectory.FullName, serverViewModel.Name + ".zip"));
                }

                DirectoryInfo serverDirectory =
                    new DirectoryInfo(Path.Combine(App.ServerPath, serverViewModel.Name));
                ZipFile.CreateFromDirectory(serverDirectory.FullName,
                    Path.Combine(deletedDirectory.FullName, serverViewModel.Name + ".zip"));
                serverDirectory.Delete(true);
                Application.Current.Dispatcher?.Invoke(()=>Entities.Remove(serverViewModel));
                serverNames.Remove(serverViewModel.Server.Name);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        private bool ChangeServerVersion(ServerVersion newVersion, ServerViewModel serverViewModel)
        {
            try
            {
                if (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
                {
                    StopServer(serverViewModel);
                    while (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
                    {
                        Thread.Sleep(500);
                    }
                }

                serverViewModel.DownloadCompleted = false;

                //Delete old server.jar
                File.Delete(Path.Combine(App.ServerPath, serverViewModel.Server.Name, "server.jar"));

                //Download new server.jar
                DirectoryInfo directoryInfo =
                    new DirectoryInfo(Path.Combine(App.ServerPath, serverViewModel.Name));
                Downloader.DownloadJarAsync(serverViewModel, directoryInfo);

                serverViewModel.Server.Version = newVersion;
                //Update Name in UI
                serverViewModel.ServerNameChanged();
                //Update stored name
                EntitySerializer.Instance.StoreEntities(Entities);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private bool DeleteDimension(MinecraftDimension dimension, Server server)
        {
            DirectoryInfo dimensionDir = GetDimensionFolder(dimension, server);
            if (!dimensionDir.Exists)
            {
                return true;
            }

            DirectoryInfo dimBackups =
                Directory.CreateDirectory(Path.Combine(App.ServerPath, server.Name, "DimensionBackups"));
            DateTime now = DateTime.Now;
            string timeStamp = now.Day + "-" + now.Month + "-" + now.Year + "_" +
                               now.Hour + "-" + now.Minute + "-" + now.Second;
            ZipFile.CreateFromDirectory(dimensionDir.FullName,
                Path.Combine(dimBackups.FullName, dimension + "_" + timeStamp + ".zip"));
            dimensionDir.Delete(true);
            return !new DirectoryInfo(dimensionDir.FullName).Exists;
        }

        private DirectoryInfo GetDimensionFolder(MinecraftDimension dimension, Server server)
        {
            string worldFolder = Path.Combine(App.ServerPath, server.Name, server.ServerSettings.LevelName);
            switch (server.Version.Type)
            {
                case ServerVersion.VersionType.Vanilla:
                    switch (dimension)
                    {
                        case MinecraftDimension.Nether:
                            return new DirectoryInfo(Path.Combine(worldFolder, "DIM-1"));
                        case MinecraftDimension.End:
                            return new DirectoryInfo(
                                Path.Combine(worldFolder, "DIM1"));
                        default:
                            throw new ArgumentException("No implementation for deletion of dimension " + dimension +
                                                        " on Vanilla servers");
                    }
                case ServerVersion.VersionType.Paper:
                    switch (dimension)
                    {
                        case MinecraftDimension.Nether:
                            return new DirectoryInfo(worldFolder + "_nether");
                        case MinecraftDimension.End:
                            return new DirectoryInfo(worldFolder+ "_the_end");
                        default:
                            throw new ArgumentException("No implementation for deletion of dimension " + dimension +
                                                        " on Paper servers");
                    }
                default:
                    throw new ArgumentException("No implementation for deletion of " + server.Version.Type +
                                                " dimensions");
            }
        }

        private bool StartServer(ServerViewModel viewModel)
        {
            ConsoleWriter.Write("\nStarting server "+viewModel.Server+" on world: "+ viewModel.Server.ServerSettings.LevelName, viewModel);
            Console.WriteLine("Starting server "+viewModel.Server.Name+" on world: "+ viewModel.Server.ServerSettings.LevelName);
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Server.Name));
            if (!directoryInfo.Exists)
            {
                return false;
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = viewModel.Server.JavaSettings.JavaPath,
                WorkingDirectory = directoryInfo.FullName,
                Arguments = "-Xmx" + viewModel.Server.JavaSettings.MaxRam + "m -Xms" +
                            viewModel.Server.JavaSettings.MinRam + "m -jar server.jar nogui",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;
            process.Start();
            new Thread(() =>
            {
                viewModel.TrackPerformance(process);
            }){IsBackground = true}.Start();
            viewModel.CurrentStatus = ServerStatus.STARTING;
            ConsoleWriter.RegisterApplication(viewModel, process.StandardOutput, process.StandardError);
            ConsoleReader consoleReader = new ConsoleReader(process.StandardInput);
            double nextRestart = AutoRestartManager.Instance.RegisterRestart(viewModel);

            viewModel.SetRestartTime(nextRestart);
            new Thread(() =>
            {
                process.WaitForExit();
                ApplicationManager.Instance.ActiveEntities.Remove(viewModel.Server);
                viewModel.CurrentStatus = ServerStatus.STOPPED;
                AutoRestartManager.Instance.DisposeRestart(viewModel);
                viewModel.SetRestartTime(-1d);
            }).Start();
            viewModel.ConsoleReader = consoleReader;
            ApplicationManager.Instance.ActiveEntities[viewModel.Server] = process;
            new Thread(() => { new QueryStatsWorker(viewModel); }){IsBackground = true}.Start();
            Console.WriteLine("Started server "+ viewModel.Server);
            
            //Register new world if created
            new Thread(() =>
            {
                while (!viewModel.ServerRunning)
                {
                    Thread.Sleep(500);
                }
                viewModel.InitializeWorldsList();
            }){IsBackground = true}.Start();
            return true;
        }

        private string RefineName(string rawName)
        {
            string result = rawName.Trim();
            if (serverNames.Contains(result))
            {
                int i = 1;
                string resultRaw = result;
                while (serverNames.Contains(result))
                {
                    result = resultRaw + "(" + i + ")";
                    i++;
                }
            }

            return result;
        }

        public bool KillEntity(EntityViewModel entityViewModel)
        {
            Process process = ApplicationManager.Instance.ActiveEntities[entityViewModel.Entity];
            try
            {
                process.Kill(entireProcessTree: true);
                ConsoleWriter.Write("Killed server "+entityViewModel.Entity, entityViewModel);
                Console.WriteLine("Killed server "+entityViewModel.Entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
            return true;
        }
    }
}