using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
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
            
            if(Servers == null)
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
            get {
                if (instance == null)
                    instance = new ServerManager();
                return instance;
            }
        }
        
        private HashSet<string> serverNames;
        
        public ObservableCollection<ServerViewModel> Servers { set; get; }
        

        public void CreateServer(ServerVersion serverVersion, ServerSettings serverSettings)
        {
            //TODO
            /*string name = serverSettings.LevelName;
            serverNames.Add(name);
            
            Server server = new Server(name, serverVersion, serverSettings);
            server.Name = name;
            server.Version = serverVersion;

            DirectoryInfo directoryInfo = Directory.CreateDirectory(name);
            WebClient webClient = new WebClient();
            webClient.DownloadFile(serverVersion.JarLink,directoryInfo.Name+"/server.jar");
            new FileWriter().WriteEula(directoryInfo.Name);
            new FileWriter().WriteServerSettings(directoryInfo.Name, serverSettings.SettingsDictionary);
            
            ServerViewModel viewModel = new ServerViewModel(server);
            Servers.Add(viewModel);
            ApplicationManager.Instance.MainViewModel.SelectedServer = viewModel;
            StartServer(viewModel);*/
        }

        public void StartServer(ServerViewModel viewModel)
        {
            //TODO
            /*viewModel.ConsoleOutList.Clear();
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
                Arguments = "-Xmx"+viewModel.Server.JavaSettings.MaxRam+"m -Xms"+viewModel.Server.JavaSettings.MinRam+"m -jar server.jar nogui",
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
            ApplicationManager.Instance.ActiveServers[viewModel.Server]= process;
            new Thread(() => { new QueryStatsWorker(viewModel); }).Start();*/
        }

        public void StopServer(Server server)
        {
            ApplicationManager.Instance.ActiveServers[server].StandardInput.WriteLine("stop");
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
    }
}
