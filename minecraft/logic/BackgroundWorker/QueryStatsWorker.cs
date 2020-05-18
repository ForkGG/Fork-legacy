using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using fork.Logic.CustomConsole;
using fork.Logic.Manager;
using fork.Logic.Model;
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
                    List<Player> players = new List<Player>(viewModel.PlayerList);
                    foreach (Player player in players)
                    {
                        if (player.Name.Equals(name, StringComparison.InvariantCulture))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Player player = Task.Run(() => PlayerManager.Instance.GetPlayer(name)).Result;
                        //Player player = new Player(name);
                        Application.Current.Dispatcher.Invoke(()=>
                        {
                            viewModel.PlayerList.Add(player);
                            Console.WriteLine("Added Player "+player.Name+" to PlayerList");
                        });
                    }
                }

                List<Player> players2 = new List<Player>(viewModel.PlayerList);
                foreach (Player player in players2)
                {
                    bool found = false;
                    foreach (string name in playerNames)
                    {
                        if (player.Name.Equals(name))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Application.Current.Dispatcher.Invoke(()=>
                        {
                            viewModel.PlayerList.Remove(player);
                            System.Console.WriteLine("Removed Player "+player.Name+" from PlayerList");
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
                    Application.Current.Dispatcher?.Invoke(() => viewModel.PlayerList.Add(p));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error finding player " + playerName);
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
                    Application.Current.Dispatcher?.Invoke(() => viewModel.PlayerList.Remove(p));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error finding player "+playerName);
                }
                return; 
            }
        }
    }
}