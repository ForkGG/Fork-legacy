using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using fork.Logic.CustomConsole;
using fork.Logic.Logging;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Model.MinecraftVersionPojo;
using fork.ViewModel;

namespace fork.Logic.BackgroundWorker
{
    public class QueryStatsWorker
    {
        public QueryStatsWorker(ServerViewModel viewModel)
        {
            //Wait for se server to start (max 60 sec)
            int retries = 0;
            while (viewModel.CurrentStatus!=ServerStatus.RUNNING&&retries<60)
            {
                Thread.Sleep(1000);
                retries++;
            }

            if (viewModel.CurrentStatus != ServerStatus.RUNNING)
            {
                Console.WriteLine("QueryStatsWorker for server ("+viewModel.Server+") stopped working");
                return;
            }

            int indexOfSep = viewModel.AddressInfo.LastIndexOf(':');
            string ip = viewModel.AddressInfo.Substring(0, indexOfSep);
            string port = viewModel.AddressInfo.Substring(indexOfSep + 1, viewModel.AddressInfo.Length - indexOfSep -1);

            Query.Query query = new Query.Query(ip,int.Parse(port));

            //Update the stats of the server while it is running
            bool QueryCrashed = false;
            while (viewModel.CurrentStatus == ServerStatus.RUNNING && query.ServerAvailable() && !QueryCrashed)
            {
                FullStats fs;
                try
                {
                    fs = query.FullServerStats();
                }
                catch (TimeoutException e)
                {
                    Console.WriteLine("Query for server "+viewModel.Server.Name+" stopped working with message: "+e.Message);
                    //TODO provide backup (/list or api.mcsrvstat.us)
                    break;
                }

                List<string> playerNames = fs.PlayerList;

                foreach (var name in playerNames)
                {
                    bool found = false;
                    List<ServerPlayer> players = new List<ServerPlayer>(viewModel.PlayerList);
                    foreach (ServerPlayer serverPlayer in players)
                    {
                        if (serverPlayer.Player.Name.Equals(name, StringComparison.InvariantCulture))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Player p = Task.Run(() => PlayerManager.Instance.GetPlayer(name)).Result;
                        ServerPlayer player = new ServerPlayer(p,viewModel,viewModel.OPList.Contains(p));
                        
                        //Player player = new Player(name);
                        Application.Current.Dispatcher.Invoke(()=>
                        {
                            viewModel.PlayerList.Add(player);
                            Console.WriteLine("Added Player "+player.Player.Name+" to PlayerList");
                        });
                    }
                }

                List<ServerPlayer> players2 = new List<ServerPlayer>(viewModel.PlayerList);
                foreach (ServerPlayer serverPlayer in players2)
                {
                    bool found = false;
                    foreach (string name in playerNames)
                    {
                        if (serverPlayer.Player.Name.Equals(name))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Application.Current.Dispatcher.Invoke(()=>
                        {
                            viewModel.PlayerList.Remove(serverPlayer);
                            Console.WriteLine("Removed Player "+serverPlayer.Player.Name+" from PlayerList");
                        });
                    }
                }
                
                Thread.Sleep(5000);
            }
            Console.WriteLine("QueryStatsWorker for server ("+viewModel.Server+") stopped working");

            if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                Console.WriteLine("Using backup system to track online users");
                ConsoleWriter.ConsoleWriteLine += HandleConsoleWrite;
            }
        }

        
        private Regex playerJoin = new Regex(@"\[([0-9]+:*)+\] \[Server thread/INFO\]: ([0-9a-zA-Z_]+) joined the game$");
        private Regex playerLeave = new Regex(@"\[([0-9]+:*)+\] \[Server thread/INFO\]: ([0-9a-zA-Z_]+) left the game$");
        private async void HandleConsoleWrite(string line, ServerViewModel viewModel)
        {
            Match joinMatch = playerJoin.Match(line);
            if (joinMatch.Success)
            {
                string playerName = joinMatch.Groups[2].Value;
                try
                {
                    Player p = await PlayerManager.Instance.GetPlayer(playerName);
                    ServerPlayer player = new ServerPlayer(p, viewModel, viewModel.OPList.Contains(p));
                    Application.Current.Dispatcher?.Invoke(() => viewModel.PlayerList.Add(player));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error finding player " + playerName);
                    ErrorLogger.Append(e);
                }
                return;
            }

            Match leaveMatch = playerLeave.Match(line);
            if (leaveMatch.Success)
            {
                string playerName = leaveMatch.Groups[2].Value;
                try
                {
                    Player p = await PlayerManager.Instance.GetPlayer(playerName);
                    ServerPlayer player = null;
                    foreach (ServerPlayer serverPlayer in viewModel.PlayerList)
                    {
                        if (serverPlayer.Player == p)
                        {
                            player = serverPlayer;
                        }
                    }
                    if (player != null)
                    {
                        Application.Current.Dispatcher?.Invoke(() => viewModel.PlayerList.Remove(player));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error finding player "+playerName);
                    ErrorLogger.Append(e);
                }
                return; 
            }
        }
    }
}