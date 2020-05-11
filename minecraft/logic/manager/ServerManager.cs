using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
using fork.Logic.Model.MinecraftVersionPojo;
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

            Servers.CollectionChanged += ServerListChanged;

            serverNames = new HashSet<string>();
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

        private HashSet<string> serverNames;

        public ObservableCollection<ServerViewModel> Servers { set; get; }


        public async Task<bool> CreateServerAsync(string serverName, ServerVersion serverVersion, ServerSettings serverSettings,
            ServerJavaSettings javaSettings, string worldPath = null)
        {
            Task<bool> t = new Task<bool>(() =>
                CreateServer(serverName, serverVersion, serverSettings, javaSettings, worldPath));
            t.Start();
            bool r = await t;
            return r;
        }

        public async Task<Server> PrepareServerImportAsync(string serverImportPath, string serverName,
            ServerVersion serverVersion)
        {
            Task<Server> t = new Task<Server>(() => PrepareServerImport(serverImportPath, serverName, serverVersion));
            t.Start();
            Server server = await t;
            return server;
        }

        public async Task<bool> ImportServerAsync(Server server, ImportViewModel importViewModel,
            ServerValidationInfo validationInfo, string originalServerDirectory, string serverName)
        {
            Task<bool> t = new Task<bool>(() =>
                ImportServer(server, importViewModel, validationInfo, originalServerDirectory, serverName));
            t.Start();
            return await t;
        }

        public async Task<bool> ImportWorldAsync(ServerVersion serverVersion, ServerSettings serverSettings,
            ServerJavaSettings javaSettings, ImportViewModel viewModel, string worldSource)
        {
            Task<bool> t = new Task<bool>(()=>
                ImportWorld(serverVersion, serverSettings, javaSettings, viewModel, worldSource));
            t.Start();
            return await t;
        }


        public void StopServer(Server server)
        {
            ApplicationManager.Instance.ActiveServers[server].StandardInput.WriteLine("stop");
        }

        public async Task<bool> StartServerAsync(ServerViewModel serverViewModel)
        {
            Task<bool> t = new Task<bool>(() => StartServer(serverViewModel));
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

        public void RestartServer(ServerViewModel serverViewModel)
        {
            StopServer(serverViewModel.Server);
            while (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
            {
                Thread.Sleep(500);
            }

            StartServer(serverViewModel);
        }


        private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Serializer.Instance.StoreServers(Servers);
        }

        private Server PrepareServerImport(string serverImportPath, string serverName, ServerVersion version)
        {
            ServerSettings settings;
            if (new FileInfo(Path.Combine(serverImportPath, "server.properties")).Exists)
            {
                var settingsDict = new FileReader().ReadServerSettings(serverImportPath);
                settings = new ServerSettings(settingsDict);
            }
            else
            {
                //TODO use world name
                settings = new ServerSettings(serverName);
            }

            Server server = new Server(serverName, version, settings, new ServerJavaSettings());
            return server;
        }

        private bool ImportServer(Server server, ImportViewModel importViewModel, ServerValidationInfo validationInfo,
            string originalServerDirectory, string serverName)
        {
            server.Name = serverName;
            string serverPath = Path.Combine(App.ApplicationPath, server.Name);
            //Create server directory
            DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(serverPath));
            
            //Import server files
            Thread copyThread = new Thread(() =>
            {
                FileImporter fileImporter = new FileImporter();
                fileImporter.CopyProgressChanged += importViewModel.CopyProgressChanged;
                fileImporter.DirectoryCopy(originalServerDirectory, serverPath, true, new List<string>{"server.jar"});
                Console.WriteLine("Finished copying server files for server "+serverName);
            });
            copyThread.Start();
            
            //Download server.jar
            Thread thread = new Thread(() =>
            {
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += importViewModel.DownloadProgressChanged;
                webClient.DownloadFileCompleted += importViewModel.DownloadCompletedHandler;
                webClient.DownloadFileAsync(new Uri(server.Version.JarLink),
                    Path.Combine(directoryInfo.FullName, "server.jar"));
            });
            thread.Start();

            if (!validationInfo.EulaTxt)
            {
                //Write Eula
                new FileWriter().WriteEula(serverPath);
            }

            //Wait for download to complete
            while (true)
            {
                if (importViewModel.DownloadCompleted)
                {
                    Console.WriteLine("Finished downloading server.jar for server " + server.Name);
                    break;
                }

                Thread.Sleep(500);
            }

            //Add server to Fork
            ServerViewModel viewModel = new ServerViewModel(server);
            Application.Current.Dispatcher.Invoke(() => Servers.Add(viewModel));
            ApplicationManager.Instance.MainViewModel.SelectedServer = viewModel;

            //TODO check if download was also complete
            return new DirectoryInfo(serverPath).Exists;
        }

        private bool ImportWorld(ServerVersion serverVersion, ServerSettings serverSettings,
            ServerJavaSettings javaSettings, ImportViewModel viewModel, string worldSource)
        {
            try
            {
                string name = serverSettings.LevelName;
                serverNames.Add(name);

                Server server = new Server(name, serverVersion, serverSettings, javaSettings);
                server.Name = name;
                server.Version = serverVersion;

                DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(App.ApplicationPath, name));
                Thread thread = new Thread(() =>
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += viewModel.DownloadProgressChanged;
                    webClient.DownloadFileCompleted += viewModel.DownloadCompletedHandler;
                    webClient.DownloadFileAsync(new Uri(serverVersion.JarLink),
                        Path.Combine(directoryInfo.FullName, "server.jar"));
                });
                thread.Start();
                new FileImporter().DirectoryCopy(worldSource, Path.Combine(directoryInfo.FullName,name), true);
                while (true)
                {
                    if (viewModel.DownloadCompleted)
                    {
                        Console.WriteLine("Finished downloading server.jar for server " + server.Name);
                        break;
                    }

                    Thread.Sleep(500);
                }

                new FileWriter().WriteEula(Path.Combine(App.ApplicationPath, directoryInfo.Name));
                new FileWriter().WriteServerSettings(Path.Combine(App.ApplicationPath, directoryInfo.Name),
                    serverSettings.SettingsDictionary);

                ServerViewModel serverViewModel = new ServerViewModel(server);
                Application.Current.Dispatcher.Invoke(() => Servers.Add(serverViewModel));
                ApplicationManager.Instance.MainViewModel.SelectedServer = serverViewModel;
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
            DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(App.ApplicationPath, serverName));
            Thread thread = new Thread(() =>
            {
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += viewModel.DownloadProgressChanged;
                webClient.DownloadFileCompleted += viewModel.DownloadCompletedHandler;
                webClient.DownloadFileAsync(new Uri(serverVersion.JarLink),
                    Path.Combine(directoryInfo.FullName, "server.jar"));
            });
            thread.Start();
            
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

        private bool DeleteServer(ServerViewModel serverViewModel)
        {
            try
            {
                if (serverViewModel.CurrentStatus != ServerStatus.STOPPED)
                {
                    StopServer(serverViewModel.Server);
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
                    StopServer(serverViewModel.Server);
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
                Thread thread = new Thread(() =>
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += serverViewModel.DownloadProgressChanged;
                    webClient.DownloadFileCompleted += serverViewModel.DownloadCompletedHandler;
                    webClient.DownloadFileAsync(new Uri(newVersion.JarLink),
                        Path.Combine(directoryInfo.FullName, "server.jar"));
                });
                thread.Start();
                while (true)
                {
                    if (serverViewModel.DownloadCompleted)
                    {
                        Console.WriteLine("Finished downloading server.jar");
                        break;
                    }

                    Thread.Sleep(500);
                }

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
            viewModel.ConsoleOutList.Clear();
            viewModel.ConsoleOutList.Add("Starting server "+viewModel.Server+" on world: "+ viewModel.Server.ServerSettings.LevelName);
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
            ConsoleWriter consoleWriter = new ConsoleWriter(viewModel, process.StandardOutput, process.StandardError);
            ConsoleReader consoleReader = new ConsoleReader(process.StandardInput, consoleWriter);
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