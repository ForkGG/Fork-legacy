using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using fork.Logic.BackgroundWorker;
using fork.Logic.CustomConsole;
using fork.Logic.ImportLogic;
using fork.Logic.Logging;
using fork.Logic.Model;
using fork.Logic.Persistence;
using fork.Logic.RoleManagement;
using fork.ViewModel;

namespace fork.Logic.Manager
{
    public sealed class ServerManager
    {
        private static ServerManager instance = null;

        private ServerManager()
        {
            if (new DirectoryInfo(Path.Combine(App.ApplicationPath, "persistence")).Exists)
            {
                Servers = Serializer.Instance.LoadServers();
            }

            if (Servers == null)
            {
                Servers = new ObservableCollection<ServerViewModel>();
            }

            foreach (ServerViewModel viewModel in Servers)
            {
                if (!viewModel.Server.Initialized)
                {
                    DownloadJarAsync(viewModel, new DirectoryInfo(Path.Combine(App.ApplicationPath, viewModel.Server.Name)));
                }
            }

            Servers.CollectionChanged += ServerListChanged;

            serverNames = new List<string>();
            foreach (ServerViewModel server in Servers)
            {
                serverNames.Add(server.Server.Name);
            }
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

        private List<string> serverNames;

        private ObservableCollection<ServerViewModel> servers;

        public ObservableCollection<ServerViewModel> Servers
        {
            get => servers ?? (servers = new ObservableCollection<ServerViewModel>());
            private set => servers = value;
        }


        public async Task<bool> CreateServerAsync(string serverName, ServerVersion serverVersion, ServerSettings serverSettings,
            ServerJavaSettings javaSettings, string worldPath = null)
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

        public async Task<bool> ImportWorldAsync(ServerViewModel viewModel, string worldSource)
        {
            Task<bool> t = new Task<bool>(()=>
                ImportWorld(viewModel, worldSource));
            t.Start();
            return await t;
        }


        public void StopServer(ServerViewModel serverViewModel)
        {
            ApplicationManager.Instance.ActiveServers[serverViewModel.Server].StandardInput.WriteLine("stop");
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

        public async Task<bool> DeleteServerAsync(ServerViewModel serverViewModel)
        {
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

        public async Task<bool> DeleteDimensionAsync(MinecraftDimension dimension, Server server)
        {
            Task<bool> t = new Task<bool>(() => DeleteDimension(dimension, server));
            t.Start();
            bool result = await t;
            return result;
        }

        public string NextDefaultServerName()
        {
            long i = 0;
            while (true)
            {
                i++;
                string name = "world" + i;
                if (!serverNames.Contains(name))
                {
                    return name;
                }
            }
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

        private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Serializer.Instance.StoreServers(Servers);
            ApplicationManager.Instance.MainViewModel.SetServerList(ref servers);
        }

        private bool ImportServer(ServerVersion version, ServerValidationInfo validationInfo,
            string originalServerDirectory, string serverName)
        {
            string serverPath = Path.Combine(App.ApplicationPath, serverName);
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

            Server server = new Server(serverName, version, settings, new ServerJavaSettings());
            serverNames.Add(serverName);
            
            //Add server to Fork
            ServerViewModel viewModel = new ServerViewModel(server);
            viewModel.StartImport();
            Application.Current.Dispatcher.Invoke(() => Servers.Add(viewModel));
            ApplicationManager.Instance.MainViewModel.SelectedServer = viewModel;
            
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
            });
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
                DirectoryInfo serverDir = new DirectoryInfo(Path.Combine(App.ApplicationPath,viewModel.Server.Name));
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

        private bool CreateServer(string serverName, ServerVersion serverVersion, ServerSettings serverSettings,
            ServerJavaSettings javaSettings, string worldPath = null)
        {
            string serverPath = Path.Combine(App.ApplicationPath, serverName);
            while (Directory.Exists(serverPath))
            {
                serverPath += "-New";
                serverName += "-New";
            }
            serverNames.Add(serverName);
            if (string.IsNullOrEmpty(serverSettings.LevelName))
            {
                serverSettings.LevelName = "world";
            }
            Server server = new Server(serverName, serverVersion, serverSettings, javaSettings);
            ServerViewModel viewModel = new ServerViewModel(server);
            Application.Current.Dispatcher.Invoke(() => Servers.Add(viewModel));
            //Select Server
            ApplicationManager.Instance.MainViewModel.SelectedServer = viewModel;
            
            //Download server.jar
            DirectoryInfo directoryInfo = Directory.CreateDirectory(serverPath);
            DownloadJarAsync(viewModel,directoryInfo);
            
            //Move World Files
            if (worldPath != null)
            {
                new FileImporter().DirectoryCopy(worldPath, Path.Combine(directoryInfo.FullName,server.ServerSettings.LevelName), true);
            }
            
            //Writing necessary files
            new FileWriter().WriteEula(Path.Combine(App.ApplicationPath, directoryInfo.Name));
            new FileWriter().WriteServerSettings(Path.Combine(App.ApplicationPath, directoryInfo.Name),
                serverSettings.SettingsDictionary);

            return true;
        }

        private void DownloadJarAsync(ServerViewModel viewModel, DirectoryInfo directoryInfo)
        {
            Thread thread = new Thread(() =>
            {
                viewModel.StartDownload();
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += viewModel.DownloadProgressChanged;
                webClient.DownloadFileCompleted += viewModel.DownloadCompletedHandler;
                webClient.DownloadFileAsync(new Uri(viewModel.Server.Version.JarLink),
                    Path.Combine(directoryInfo.FullName, "server.jar"));
            });
            thread.Start();
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
                    Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "backup", "deleted"));
                if (File.Exists(Path.Combine(deletedDirectory.FullName, serverViewModel.Server.Name + ".zip")))
                {
                    File.Delete(Path.Combine(deletedDirectory.FullName, serverViewModel.Server.Name + ".zip"));
                }

                DirectoryInfo serverDirectory =
                    new DirectoryInfo(Path.Combine(App.ApplicationPath, serverViewModel.Server.Name));
                ZipFile.CreateFromDirectory(serverDirectory.FullName,
                    Path.Combine(deletedDirectory.FullName, serverViewModel.Server.Name + ".zip"));
                serverDirectory.Delete(true);
                Servers.Remove(serverViewModel);
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
                File.Delete(Path.Combine(App.ApplicationPath, serverViewModel.Server.Name, "server.jar"));

                //Download new server.jar
                DirectoryInfo directoryInfo =
                    new DirectoryInfo(Path.Combine(App.ApplicationPath, serverViewModel.Server.Name));
                DownloadJarAsync(serverViewModel, directoryInfo);

                serverViewModel.Server.Version = newVersion;
                //Update Name in UI
                serverViewModel.ServerNameChanged();
                //Update stored name
                Serializer.Instance.StoreServers(Servers);

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
                Directory.CreateDirectory(Path.Combine(App.ApplicationPath, server.Name, "DimensionBackups"));
            string timeStamp = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "_" +
                               DateTime.Now.Hour + "-" + DateTime.Now.Minute;
            ZipFile.CreateFromDirectory(dimensionDir.FullName,
                Path.Combine(dimBackups.FullName, dimension + "_" + timeStamp + ".zip"));
            dimensionDir.Delete(true);
            return !dimensionDir.Exists;
        }

        private DirectoryInfo GetDimensionFolder(MinecraftDimension dimension, Server server)
        {
            switch (server.Version.Type)
            {
                case ServerVersion.VersionType.Vanilla:
                    switch (dimension)
                    {
                        case MinecraftDimension.Nether:
                            return new DirectoryInfo(Path.Combine(App.ApplicationPath, server.Name, server.Name,
                                "DIM-1"));
                        case MinecraftDimension.End:
                            return new DirectoryInfo(
                                Path.Combine(App.ApplicationPath, server.Name, server.Name, "DIM1"));
                        default:
                            throw new ArgumentException("No implementation for deletion of dimension " + dimension +
                                                        " on Vanilla servers");
                    }
                case ServerVersion.VersionType.Paper:
                    switch (dimension)
                    {
                        case MinecraftDimension.Nether:
                            return new DirectoryInfo(Path.Combine(App.ApplicationPath, server.Name,
                                server.Name + "_nether"));
                        case MinecraftDimension.End:
                            return new DirectoryInfo(Path.Combine(App.ApplicationPath, server.Name,
                                server.Name + "_the_end"));
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
            viewModel.ConsoleOutList.Add("\nStarting server "+viewModel.Server+" on world: "+ viewModel.Server.ServerSettings.LevelName);
            Console.WriteLine("Starting server "+viewModel.Server.Name+" on world: "+ viewModel.Server.ServerSettings.LevelName);
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ApplicationPath, viewModel.Server.Name));
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
                FileName = "java.exe",
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
            }).Start();
            viewModel.CurrentStatus = ServerStatus.STARTING;
            ConsoleWriter.RegisterApplication(viewModel, process.StandardOutput, process.StandardError);
            ConsoleReader consoleReader = new ConsoleReader(process.StandardInput);
            double nextRestart = AutoRestartManager.Instance.RegisterRestart(viewModel);

            viewModel.SetRestartTime(nextRestart);
            new Thread(() =>
            {
                process.WaitForExit();
                ApplicationManager.Instance.ActiveServers.Remove(viewModel.Server);
                viewModel.CurrentStatus = ServerStatus.STOPPED;
                AutoRestartManager.Instance.DisposeRestart(viewModel);
                viewModel.SetRestartTime(-1d);
            }).Start();
            viewModel.ConsoleReader = consoleReader;
            ApplicationManager.Instance.ActiveServers[viewModel.Server] = process;
            new Thread(() => { new QueryStatsWorker(viewModel); }).Start();
            Console.WriteLine("Started server "+ viewModel.Server);
            
            //Register new world if created
            new Thread(() =>
            {
                while (!viewModel.ServerRunning)
                {
                    Thread.Sleep(500);
                }
                viewModel.InitializeWorldsList();
            }).Start();
            return true;
        }

        public bool KillServer(ServerViewModel serverViewModel)
        {
            Process process = ApplicationManager.Instance.ActiveServers[serverViewModel.Server];
            try
            {
                process?.Kill();
                serverViewModel.ConsoleOutList.Add("Killed server "+serverViewModel.Server);
                Console.WriteLine("Killed server "+serverViewModel.Server);
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