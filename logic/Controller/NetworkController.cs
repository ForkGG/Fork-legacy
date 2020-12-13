using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using Fork.Logic.CustomConsole;
using Fork.Logic.ImportLogic;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.ProxyModels;
using Fork.Logic.Model.ProxyModels;
using Fork.Logic.Model.ServerConsole;
using Fork.Logic.WebRequesters;
using Fork.ViewModel;
using Newtonsoft.Json;

namespace Fork.Logic.Controller
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
            string serverPath = Path.Combine(App.ServerPath, networkName);
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
            //new FileWriter().WriteServerSettings(Path.Combine(App.ServerPath, directoryInfo.Name), serverSettings.SettingsDictionary);

            return true;
        }

        public bool StartNetwork(NetworkViewModel viewModel, bool startServers)
        {
            ConsoleWriter.Write("\n Starting network "+viewModel.Entity.Name, viewModel);
            Console.WriteLine("Starting network "+viewModel.Entity.Name);
            if (startServers)
            {
                if (viewModel.Servers.Count == 0)
                {
                    ConsoleWriter.Write("WARNING: This network contains no Servers. Players will not be able to join.", viewModel);
                }
                else
                {
                    ConsoleWriter.Write("Starting all "+viewModel.Servers.Count+" servers of this network...", viewModel);
                    foreach (NetworkServer networkServer in viewModel.Servers)
                    {
                        if (networkServer is NetworkForkServer networkForkServer)
                        {
                            ServerViewModel serverViewModel = networkForkServer.ServerViewModel;
                            if (serverViewModel.CurrentStatus == ServerStatus.STOPPED)
                            {
                                ConsoleWriter.Write("\nStarting server " + serverViewModel.Server + " on world: " +
                                                    serverViewModel.Server.ServerSettings.LevelName, viewModel);
                                ServerManager.Instance.StartServerAsync(serverViewModel);
                            } else if (serverViewModel.CurrentStatus == ServerStatus.STARTING)
                            {
                                ConsoleWriter.Write("Server "+serverViewModel.Server+" is already starting.", viewModel);
                            } else if (serverViewModel.CurrentStatus == ServerStatus.RUNNING)
                            {                            
                                ConsoleWriter.Write("Server "+serverViewModel.Server+" is already running.", viewModel);
                            }
                        }
                        else
                        {
                            ConsoleWriter.Write("Server "+networkServer.Name+" can't be started automatically because it is no Fork server.", viewModel);
                        }
                    }
                }
            }
            else
            {
                ConsoleWriter.Write("Make sure that at least one server configured in the settings is running, else Players won't be able to join this network.", viewModel);
            }
            
            //Start proxy server
            ConsoleWriter.Write("Starting proxy server...", viewModel);
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Network.Name));
            if (!directoryInfo.Exists)
            {
                ConsoleWriter.Write("ERROR: Can't find network directory: "+directoryInfo.FullName, viewModel);
                return false;
            }
            
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                FileName = viewModel.Network.JavaSettings.JavaPath,
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
            }){IsBackground = true}.Start();
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
                ConsoleWriter.Write("Stopping all "+viewModel.Servers.Count+" servers of this network...", viewModel);
                foreach (NetworkServer networkServer in viewModel.Servers)
                {
                    if (networkServer is NetworkForkServer networkForkServer)
                    {
                        ServerViewModel serverViewModel = networkForkServer.ServerViewModel;
                        if (serverViewModel.CurrentStatus == ServerStatus.RUNNING)
                        {
                            ConsoleWriter.Write("Stopping server " + serverViewModel.Server, viewModel);
                            ServerManager.Instance.StopServer(serverViewModel);
                        } else if (serverViewModel.CurrentStatus == ServerStatus.STOPPED)
                        {
                            ConsoleWriter.Write("Server "+serverViewModel.Server+" is already stopped.", viewModel);
                        } else if (serverViewModel.CurrentStatus == ServerStatus.STARTING)
                        {                            
                            ConsoleWriter.Write("Server "+serverViewModel.Server+" is currently starting (manual stop needed)", viewModel);
                        }
                    }
                    else
                    {
                        ConsoleWriter.Write("Server "+networkServer.Name+" can't be stopped automatically because it is no Fork server.", viewModel);
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
                    Directory.CreateDirectory(Path.Combine(App.ServerPath, "backup", "deleted"));
                if (File.Exists(Path.Combine(deletedDirectory.FullName, networkViewModel.Name + ".zip")))
                {
                    File.Delete(Path.Combine(deletedDirectory.FullName, networkViewModel.Name + ".zip"));
                }

                DirectoryInfo serverDirectory =
                    new DirectoryInfo(Path.Combine(App.ServerPath, networkViewModel.Name));
                ZipFile.CreateFromDirectory(serverDirectory.FullName,
                    Path.Combine(deletedDirectory.FullName, networkViewModel.Name + ".zip"));
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

        public bool CloneNetwork(NetworkViewModel viewModel, List<string> usedEntityNames)
        {
            if (viewModel.CurrentStatus != ServerStatus.STOPPED)
            {
                StopNetwork(viewModel, false);
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
                string newName = RefineName(viewModel.Name+"-Clone", usedEntityNames);
                
                //Better to use a object copy function
                string oldNetworkJson = JsonConvert.SerializeObject(viewModel.Network);
                Network newNetwork = JsonConvert.DeserializeObject<Network>(oldNetworkJson);
                
                newNetwork.Name = newName;
                newNetwork.UID = Guid.NewGuid().ToString();
                NetworkViewModel newNetworkViewModel = new NetworkViewModel(newNetwork);

                string newNetworkPath = Path.Combine(App.ServerPath, newName);
                newNetworkViewModel.StartImport();
                Application.Current.Dispatcher?.Invoke(() => ServerManager.Instance.Entities.Add(newNetworkViewModel));
                ApplicationManager.Instance.MainViewModel.SelectedEntity = newNetworkViewModel;
            
                //Create server directory
                Directory.CreateDirectory(newNetworkPath);
            
                //Import server files
                Thread copyThread = new Thread(() =>
                {
                    FileImporter fileImporter = new FileImporter();
                    fileImporter.CopyProgressChanged += newNetworkViewModel.CopyProgressChanged;
                    fileImporter.DirectoryCopy(directoryInfo.FullName, newNetworkPath, true, new List<string>{"server.jar"});
                    Console.WriteLine("Finished copying server files for server "+newNetworkPath);
                    newNetworkViewModel.FinishedCopying();
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

        public bool RenameNetwork(NetworkViewModel viewModel, string newName)
        {
            if (viewModel.CurrentStatus != ServerStatus.STOPPED)
            {
                StopNetwork(viewModel, true);
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
                            ConsoleWriter.Write("Killed server "+server.Server, viewModel);
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