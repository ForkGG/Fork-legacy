using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using fork.Logic.CustomConsole;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Model.ProxyModels;
using Fork.Logic.Model.ProxyModels;
using fork.Logic.WebRequesters;
using fork.ViewModel;

namespace fork.Logic.Controller
{
    /// <summary>
    /// Backend Logic for managing Proxy Networks
    /// Starting, Stopping, Creating, Restarting, ...
    /// </summary>
    public class NetworkController
    {
        public bool CreateNetwork(string networkName, ServerVersion.VersionType networkType, JavaSettings javaSettings, List<string> usedServerNames)
        {
            if (networkType != ServerVersion.VersionType.Waterfall)
            {
                return false;
            }
            networkName = RefineName(networkName, usedServerNames);
            string serverPath = Path.Combine(App.ApplicationPath, networkName);
            DirectoryInfo directoryInfo = Directory.CreateDirectory(serverPath);
            Network network = new Network(networkName, networkType, javaSettings, VersionManager.Instance.WaterfallVersion);
            NetworkViewModel viewModel = new NetworkViewModel(network);
            ServerManager.Instance.AddEntity(viewModel);
            //Select Server
            ApplicationManager.Instance.MainViewModel.SelectedEntity = viewModel;
            
            //Download server.jar
            Downloader.DownloadJarAsync(viewModel,directoryInfo);

            //Writing necessary files
            //TODO write settings
            //new FileWriter().WriteServerSettings(Path.Combine(App.ApplicationPath, directoryInfo.Name), serverSettings.SettingsDictionary);

            return true;
        }

        public bool StartNetwork(NetworkViewModel viewModel, bool startServers)
        {
            viewModel.ConsoleOutList.Add("\n Starting network "+viewModel.Entity.Name);
            Console.WriteLine("Starting network "+viewModel.Entity.Name);
            if (startServers)
            {
                if (viewModel.Servers.Count == 0)
                {
                    viewModel.ConsoleOutList.Add("WARNING: This network contains no Servers. Players will not be able to join.");
                }
                else
                {
                    viewModel.ConsoleOutList.Add("Starting all "+viewModel.Servers.Count+" servers of this network...");
                    foreach (NetworkServer networkServer in viewModel.Servers)
                    {
                        if (networkServer is NetworkForkServer networkForkServer)
                        {
                            ServerViewModel serverViewModel = networkForkServer.ServerViewModel;
                            if (serverViewModel.CurrentStatus == ServerStatus.STOPPED)
                            {
                                viewModel.ConsoleOutList.Add("\nStarting server " + serverViewModel.Server + " on world: " +
                                                             serverViewModel.Server.ServerSettings.LevelName);
                                ServerManager.Instance.StartServerAsync(serverViewModel);
                            } else if (serverViewModel.CurrentStatus == ServerStatus.STARTING)
                            {
                                viewModel.ConsoleOutList.Add("Server "+serverViewModel.Server+" is already starting.");
                            } else if (serverViewModel.CurrentStatus == ServerStatus.RUNNING)
                            {                            
                                viewModel.ConsoleOutList.Add("Server "+serverViewModel.Server+" is already running.");
                            }
                        }
                        else
                        {
                            viewModel.ConsoleOutList.Add("Server "+networkServer.Name+" can't be started automatically because it is no Fork server.");
                        }
                    }
                }
            }
            else
            {
                viewModel.ConsoleOutList.Add("Make sure that at least one server configured in the settings is running, else Players won't be able to join this network.");
            }
            
            //Start proxy server
            viewModel.ConsoleOutList.Add("Starting proxy server...");
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ApplicationPath, viewModel.Network.Name));
            if (!directoryInfo.Exists)
            {
                viewModel.ConsoleOutList.Add("ERROR: Can't find network directory: "+directoryInfo.FullName);
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
                Arguments = "-Xmx" + viewModel.Network.JavaSettings.MaxRam + "m -Xms" +
                            viewModel.Network.JavaSettings.MinRam + "m -jar server.jar nogui",
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
            viewModel.ConsoleReader = consoleReader;
            new Thread(() =>
            {
                process.WaitForExit();
                ApplicationManager.Instance.ActiveEntities.Remove(viewModel.Entity);
                viewModel.CurrentStatus = ServerStatus.STOPPED;
            }).Start();
            ApplicationManager.Instance.ActiveEntities[viewModel.Network] = process;
            Console.WriteLine("Started network "+ viewModel.Network);

            return true;
        }

        public bool StopNetwork(NetworkViewModel viewModel, bool stopServers)
        {
            Process p = ApplicationManager.Instance.ActiveEntities[viewModel.Network];
            if (p == null)
            {
                return false;
            }
            p.StandardInput.WriteLine("end");

            if (stopServers)
            {
                viewModel.ConsoleOutList.Add("Stopping all "+viewModel.Servers.Count+" servers of this network...");
                foreach (NetworkServer networkServer in viewModel.Servers)
                {
                    if (networkServer is NetworkForkServer networkForkServer)
                    {
                        ServerViewModel serverViewModel = networkForkServer.ServerViewModel;
                        if (serverViewModel.CurrentStatus == ServerStatus.RUNNING)
                        {
                            viewModel.ConsoleOutList.Add("Stopping server " + serverViewModel.Server);
                            ServerManager.Instance.StopServer(serverViewModel);
                        } else if (serverViewModel.CurrentStatus == ServerStatus.STOPPED)
                        {
                            viewModel.ConsoleOutList.Add("Server "+serverViewModel.Server+" is already stopped.");
                        } else if (serverViewModel.CurrentStatus == ServerStatus.STARTING)
                        {                            
                            viewModel.ConsoleOutList.Add("Server "+serverViewModel.Server+" is currently starting (manual stop needed)");
                        }
                    }
                    else
                    {
                        viewModel.ConsoleOutList.Add("Server "+networkServer.Name+" can't be stopped automatically because it is no Fork server.");
                    }
                }
            }

            while (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                Thread.Sleep(500);
            }
            
            return true;
        }
        
        public bool DeleteNetwork(NetworkViewModel networkViewModel)
        {
            try
            {
                if (networkViewModel.CurrentStatus != ServerStatus.STOPPED)
                {
                    StopNetwork(networkViewModel, false);
                    while (networkViewModel.CurrentStatus != ServerStatus.STOPPED)
                    {
                        Thread.Sleep(500);
                    }
                }

                DirectoryInfo deletedDirectory =
                    Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "backup", "deleted"));
                if (File.Exists(Path.Combine(deletedDirectory.FullName, networkViewModel.Network.Name + ".zip")))
                {
                    File.Delete(Path.Combine(deletedDirectory.FullName, networkViewModel.Network.Name + ".zip"));
                }

                DirectoryInfo serverDirectory =
                    new DirectoryInfo(Path.Combine(App.ApplicationPath, networkViewModel.Network.Name));
                ZipFile.CreateFromDirectory(serverDirectory.FullName,
                    Path.Combine(deletedDirectory.FullName, networkViewModel.Network.Name + ".zip"));
                serverDirectory.Delete(true);
                Application.Current.Dispatcher?.Invoke(()=>ServerManager.Instance.RemoveEntity(networkViewModel));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public bool KillNetwork(NetworkViewModel viewModel, bool killServer)
        {
            bool success = ServerManager.Instance.KillEntity(viewModel);

            if (killServer && success)
            {
                foreach (NetworkServer networkServer in viewModel.Servers)
                {
                    if (networkServer is NetworkForkServer networkForkServer)
                    {
                        ServerViewModel server = networkForkServer.ServerViewModel;
                        if (server.CurrentStatus == ServerStatus.RUNNING || server.CurrentStatus == ServerStatus.STARTING)
                        {
                            ServerManager.Instance.KillEntity(server);
                            viewModel.ConsoleOutList.Add("Killed server "+server.Server);
                        }
                    }
                }
            }

            return success;
        }
        
        
        private string RefineName(string rawName, List<string> serverNames)
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
    }
}