using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using fork.Annotations;
using fork.Logic.Model;
using fork.ViewModel;

namespace fork.Logic.Manager
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
            ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput.WriteLine("whitelist add "+name);
        }

        public void UnWhitelistPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput.WriteLine("whitelist remove "+name);
        }

        public void KickPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput.WriteLine("kick "+name);
        }

        public void BanPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput.WriteLine("ban "+name);
        }

        public void UnBanPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput.WriteLine("pardon "+name);
        }

        public void OpPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput.WriteLine("op "+name);
        }

        public void DeopPlayer(ServerViewModel viewModel, string name)
        {
            if (viewModel.CurrentStatus!= ServerStatus.RUNNING)
            {
                Console.WriteLine("Can only change roles while server is running!");
                return;
            }
            ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput.WriteLine("deop "+name);
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

        public List<ServerPlayer> GetInitialPlayerList(ServerViewModel viewModel)
        {
            HashSet<string> playerIDsToAdd = new HashSet<string>();
            foreach (World world in viewModel.Worlds)
            {
                if (world.Directory.Exists)
                {
                    DirectoryInfo playerData = new DirectoryInfo(Path.Combine(world.Directory.FullName,"playerdata"));
                    if (playerData.Exists)
                    {
                        foreach (FileInfo fileInfo in playerData.EnumerateFiles())
                        {
                            string uuid = fileInfo.Name;
                            uuid = uuid.Replace("-", "").Replace(".dat", "");
                            playerIDsToAdd.Add(uuid);
                        }
                    }
                }
            }

            List<ServerPlayer> result = new List<ServerPlayer>();
            foreach (string uuid in playerIDsToAdd)
            {
                Player p = GetPlayerFromUUID(uuid);
                ServerPlayer player = new ServerPlayer(p,viewModel,viewModel.OPList.Contains(p), false);
                result.Add(player);
            }

            return result;
        }

        private Player CreatePlayer(string name)
        {
            return new Player(name);
        }
        
        private Player GetPlayerFromUUID(string uuid)
        {
            foreach (var player in PlayerSet)
            {
                if (player.Uid.Equals(uuid))
                {
                    return player;
                }
            }
            Player p = CreatePlayerFromUUID(uuid);
            PlayerSet.Add(p);
            SafePlayersToFile();
            return p;
        }
        private Player CreatePlayerFromUUID(string uuid)
        {
            return new Player(uuid, true);
        }

        private void SafePlayersToFile()
        {
            string json = JsonConvert.SerializeObject(PlayerSet, Formatting.Indented);
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