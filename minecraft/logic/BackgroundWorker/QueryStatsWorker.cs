using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
                    return;
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

            System.Console.WriteLine("QueryStatsWorker for server ("+viewModel.Server+") stopped working");
        }
    }
}