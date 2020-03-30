using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Instrumentation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using nihilus.Annotations;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.Logic.Manager
{
    public sealed class PlayerManager
    {
        #region Singleton
        //Lock to ensure Singleton pattern
        private static object myLock = new object();
        private static PlayerManager instance;
        public static PlayerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (myLock)
                    {
                        if (instance == null)
                        {
                            instance = new PlayerManager();
                        }
                    }
                }
                return instance;
            }
        }
        private PlayerManager()
        {
            PlayerSet = InitializePlayerSet();
        }
        #endregion

        private HashSet<Player> PlayerSet;
        private string PlayerJsonPath;
        
        public void WhitelistPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveServers[viewModel.Server].StandardInput.WriteLine("whitelist add "+name);
        }

        public void UnWhitelistPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveServers[viewModel.Server].StandardInput.WriteLine("whitelist remove "+name);
        }

        public void KickPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveServers[viewModel.Server].StandardInput.WriteLine("kick "+name);
        }

        public void BanPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveServers[viewModel.Server].StandardInput.WriteLine("ban "+name);
        }

        public void UnBanPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveServers[viewModel.Server].StandardInput.WriteLine("pardon "+name);
        }

        public void OpPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveServers[viewModel.Server].StandardInput.WriteLine("op "+name);
        }

        public void DeopPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveServers[viewModel.Server].StandardInput.WriteLine("deop "+name);
        }

        public async Task<Player> GetPlayer(string name)
        {
            foreach (var player in PlayerSet)
            {
                if (player.Name.Equals(name))
                {
                    return player;
                }
            }
            Player p = Task.Run(()=>CreatePlayer(name)).Result;
            PlayerSet.Add(p);
            SafePlayersToFile();
            return p;
        }

        private Player CreatePlayer(string name)
        {
            return new Player(name);
        }

        private void SafePlayersToFile()
        {
            string json = JsonConvert.SerializeObject(PlayerSet);
            File.WriteAllText(PlayerJsonPath,json);
        }

        private HashSet<Player> InitializePlayerSet()
        {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "players"));
            PlayerJsonPath = Path.Combine(directoryInfo.FullName, "players.json");
            if (!File.Exists(PlayerJsonPath))
            {
                return new HashSet<Player>();
            }
            string json = File.ReadAllText(PlayerJsonPath);
            return JsonConvert.DeserializeObject<HashSet<Player>>(json);
        }
    }
}