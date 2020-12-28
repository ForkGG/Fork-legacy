using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fork.Logic.Model;
using Fork.Logic.Model.ServerConsole;
using Fork.ViewModel;

namespace Fork.Logic.Manager
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
            
            //Check for old player data and update
            bool anyUpdate = false;
            foreach (var player in PlayerSet.Where(player => !player.offlineChar && (DateTime.Now - player.LastUpdated).TotalDays > 7d))
            {
                player.Update();
                Console.WriteLine("Updated data for player "+player);
                anyUpdate = true;
            }
            if (anyUpdate)
            {
                SafePlayersToFile();
            }
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
            if (!CheckPlayerName(name))
            {
                Console.WriteLine("Tried to retrieve invalid player ("+name+") from database. Skipping...");
                return null;
            }
            
            var existingPlayers = PlayerSet.Where(player => player.Name.Equals(name)).ToArray();
            if (existingPlayers.Any())
            {
                return existingPlayers.First();
            }
            Player p = await Task.Run(()=>CreatePlayer(name));
            PlayerSet.Add(p);
            SafePlayersToFile();
            return p;
        }

        public List<ServerPlayer> GetInitialPlayerList(ServerViewModel viewModel)
        {
            lock (Instance)
            {
                HashSet<string> playerIDsToAdd = new HashSet<string>();
                foreach (World world in viewModel.Worlds)
                {
                    if (world.Directory.Exists)
                    {
                        DirectoryInfo playerData = new DirectoryInfo(Path.Combine(world.Directory.FullName,"playerdata"));
                        if (playerData.Exists)
                        {
                            foreach (string fileName in Directory.GetFiles(playerData.FullName, "*.dat", SearchOption.TopDirectoryOnly))
                            {
                                string uuid = new FileInfo(fileName).Name.Replace("-", "").Replace(".dat", "");
                                if (ValidateUuid(uuid))
                                {
                                    playerIDsToAdd.Add(uuid);
                                }
                            }
                        }
                    }
                }

                List<ServerPlayer> result = new List<ServerPlayer>();
                foreach (string uuid in playerIDsToAdd)
                {
                    Player p = GetPlayerFromUUID(uuid);
                    if (p != null)
                    {
                        ServerPlayer player = new ServerPlayer(p,viewModel,viewModel.OPList.Contains(p), false);
                        result.Add(player);
                    }
                }

                return result;
            }
        }

        private Player CreatePlayer(string name)
        {
            return new Player(name);
        }
        
        private Player GetPlayerFromUUID(string uuid)
        {
            var existingPlayers = PlayerSet.Where(player => player.Uid.Equals(uuid)).ToArray();
            if (existingPlayers.Any())
            {
                return existingPlayers.First();
            }
            
            Player p = CreatePlayerFromUUID(uuid);
            if (p != null)
            {
                PlayerSet.Add(p);
                SafePlayersToFile();
            }
            else
            {
                DirectoryInfo playerDir = new DirectoryInfo(Path.Combine(App.ApplicationPath, "players", uuid));
                if (playerDir.Exists)
                {
                    playerDir.Delete(true);
                }
            }
            return p;
        }
        private Player CreatePlayerFromUUID(string uuid)
        {
            Player p = new Player(uuid, true);
            return CheckPlayerName(p.Name) ? p : null;
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

        /// <summary>
        /// Check if a given Player has a valid name
        /// This is used to remove players with UUID name (This happens if UUID is not in mojang database)
        /// </summary>
        /// <param name="playerName">The player name to check</param>
        /// <returns>Validity of Player.Name</returns>
        private bool CheckPlayerName(string playerName)
        {
            Regex regex = new Regex(@"^[A-Za-z0-9_]{3,16}$");
            return regex.Matches(playerName).Count != 0;
        }

        private bool ValidateUuid(string uuid)
        {
            try
            {
                byte[] ba = Enumerable.Range(0, uuid.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(uuid.Substring(x, 2), 16))
                    .ToArray();
                
                return ba.Length == 16;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}