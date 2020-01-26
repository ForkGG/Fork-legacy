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
using nihilus.Logic.BackgroundWorker;
using nihilus.Logic.CustomConsole;
using nihilus.Logic.Model;
using nihilus.Logic.Persistence;
using nihilus.ViewModel;

namespace nihilus.Logic.Manager
{
    public sealed class ServerManager
    {
        private static ServerManager instance = null;

        private ServerManager()
        {
            if (new DirectoryInfo("persistence").Exists)
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


        public async Task<bool> CreateServerAsync(ServerVersion serverVersion, ServerSettings serverSettings,
            ServerJavaSettings javaSettings, AddServerViewModel addServerViewModel)
        {
            Task<bool> t = new Task<bool>(() => CreateServer(serverVersion, serverSettings, javaSettings, addServerViewModel));
            t.Start();
            bool r = await t;
            return r;
        }

        public void StartServer(ServerViewModel viewModel)
        {
            viewModel.ConsoleOutList.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(viewModel.Server.Name);
            if (!directoryInfo.Exists)
            {
                return;
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = "java.exe",
                WorkingDirectory = directoryInfo.Name,
                Arguments = "-Xmx" + viewModel.Server.JavaSettings.MaxRam + "m -Xms" +
                            viewModel.Server.JavaSettings.MinRam + "m -jar server.jar nogui",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;
            process.Start();
            viewModel.TrackPerformance(process);
            viewModel.CurrentStatus = ServerStatus.STARTING;
            ConsoleWriter consoleWriter = new ConsoleWriter(viewModel, process.StandardOutput, process.StandardError);
            ConsoleReader consoleReader = new ConsoleReader(process.StandardInput, consoleWriter);
            new Thread(() =>
            {
                process.WaitForExit();
                ApplicationManager.Instance.ActiveServers.Remove(viewModel.Server);
                viewModel.CurrentStatus = ServerStatus.STOPPED;
            }).Start();
            viewModel.ConsoleReader = consoleReader;
            ApplicationManager.Instance.ActiveServers[viewModel.Server] = process;
            new Thread(() => { new QueryStatsWorker(viewModel); }).Start();
        }

        public void StopServer(Server server)
        {
            ApplicationManager.Instance.ActiveServers[server].StandardInput.WriteLine("stop");
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
            Task<bool> t = new Task<bool>(() => ChangeServerVersion(newVersion,serverViewModel));
            t.Start();
            bool result = await t;
            return result;
        }

        public async Task<bool> DeleteDimensionAsync(string dimension, Server server)
        {
            Task<bool> t = new Task<bool>(()=> DeleteDimension(dimension,server));
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


        private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Serializer.Instance.StoreServers(Servers);
        }

        private bool CreateServer(ServerVersion serverVersion, ServerSettings serverSettings,
            ServerJavaSettings javaSettings, AddServerViewModel addServerViewModel)
        {
            try
            {
                string name = serverSettings.LevelName;
                serverNames.Add(name);

                Server server = new Server(name, serverVersion, serverSettings, javaSettings);
                server.Name = name;
                server.Version = serverVersion;

                DirectoryInfo directoryInfo = Directory.CreateDirectory(name);
                Thread thread = new Thread(() =>
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += addServerViewModel.DownloadProgressChanged;
                    webClient.DownloadFileCompleted += addServerViewModel.DownloadCompletedHandler;
                    webClient.DownloadFileAsync(new Uri(serverVersion.JarLink), directoryInfo.Name + "/server.jar");
                });
                thread.Start();
                while (true)
                {
                    if (addServerViewModel.DownloadCompleted)
                    {
                        Console.WriteLine("Finished downloading server.jar");
                        break;
                    }
                    Thread.Sleep(500);
                }

                new FileWriter().WriteEula(directoryInfo.Name);
                new FileWriter().WriteServerSettings(directoryInfo.Name, serverSettings.SettingsDictionary);

                ServerViewModel viewModel = new ServerViewModel(server);
                Application.Current.Dispatcher.Invoke(new Action(() => Servers.Add(viewModel)));
                ApplicationManager.Instance.MainViewModel.SelectedServer = viewModel;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
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

                DirectoryInfo deletedDirectory = Directory.CreateDirectory(Path.Combine("backup","deleted"));
                if (File.Exists(Path.Combine(deletedDirectory.FullName,serverViewModel.Server.Name+".zip")))
                {
                    File.Delete(Path.Combine(deletedDirectory.FullName,serverViewModel.Server.Name+".zip"));
                }
                DirectoryInfo serverDirectory = new DirectoryInfo(serverViewModel.Server.Name);
                ZipFile.CreateFromDirectory(serverViewModel.Server.Name,Path.Combine(deletedDirectory.FullName,serverViewModel.Server.Name+".zip"));
                serverDirectory.Delete(true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                
                //Delete old server.jar
                File.Delete(Path.Combine(serverViewModel.Server.Name,"server.jar"));
                
                //Download new server.jar
                DirectoryInfo directoryInfo = new DirectoryInfo(serverViewModel.Server.Name);
                Thread thread = new Thread(() =>
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadProgressChanged += serverViewModel.DownloadProgressChanged;
                    webClient.DownloadFileCompleted += serverViewModel.DownloadCompletedHandler;
                    webClient.DownloadFileAsync(new Uri(newVersion.JarLink), directoryInfo.Name + "/server.jar");
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
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            } 
        }

        private bool DeleteDimension(string dimension, Server server)
        {
            DirectoryInfo dimensionDir = new DirectoryInfo(Path.Combine(server.Name,server.Name,dimension));
            if (!dimensionDir.Exists)
            {
                return true;
            }
            DirectoryInfo dimBackups = Directory.CreateDirectory(Path.Combine(server.Name, server.Name, "DimensionBackups"));
            string timeStamp = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "_" +
                               DateTime.Now.Hour + "-" + DateTime.Now.Minute;
            ZipFile.CreateFromDirectory(dimensionDir.FullName, Path.Combine(dimBackups.FullName, dimension +"_" + timeStamp + ".zip"));
            dimensionDir.Delete(true);
            return !dimensionDir.Exists;
        }
    }
}