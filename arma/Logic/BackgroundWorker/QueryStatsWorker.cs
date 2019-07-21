using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.Logic.BackgroundWorker
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
                System.Console.WriteLine("QueryStatsWorker for server ("+viewModel.Server+") stopped working");
                return;
            }

            string[] addressinfo = viewModel.AddressInfo.Split(':');
            Query.Query query = new Query.Query(addressinfo[0],int.Parse(addressinfo[1]));

            //Update the stats of the server while it is running
            while (viewModel.CurrentStatus == ServerStatus.RUNNING && query.ServerAvailable())
            {
                FullStats fs = query.FullServerStats();
                List<string> playerNames = fs.PlayerList;

                foreach (var name in playerNames)
                {
                    bool found = false;
                    List<Player> players = new List<Player>(viewModel.PlayerList);
                    foreach (Player player in players)
                    {
                        if (player.Name.Equals(name))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Player player = new Player(name);
                        Application.Current.Dispatcher.Invoke(()=>
                        {
                            viewModel.PlayerList.Add(player);
                            System.Console.WriteLine("Added Player "+player.Name+" to PlayerList");
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