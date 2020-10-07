using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Fork.Logic.CustomConsole;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.ViewModel;

namespace Fork.Logic.BackgroundWorker
{
    public class QueryStatsWorker
    {
        public QueryStatsWorker(ServerViewModel viewModel)
        {
            //Wait for se server to start (max 60 sec)
            int retries = 0;
            while (viewModel.CurrentStatus != ServerStatus.RUNNING && retries < 60)
            {
                Thread.Sleep(1000);
                retries++;
            }

            if (viewModel.CurrentStatus != ServerStatus.RUNNING)
            {
                Console.WriteLine("QueryStatsWorker for server (" + viewModel.Server + ") stopped working");
                return;
            }

            if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                Console.WriteLine("Registering Join/Leave handler for server " + viewModel.Server);
                ConsoleWriter.ConsoleWriteLine += HandleConsoleWrite;
            }
        }


        private Regex playerJoin =
            new Regex(@"\[([0-9]+:*)+\] \[Server thread/INFO\]: ([0-9a-zA-Z_]+) joined the game$");
        private Regex playerJoinPaper =
            new Regex(@"\[([0-9]+:*)+ INFO\]: ([0-9a-zA-Z_]+) joined the game$");
        private Regex playerJoinPaper2 =
            new Regex(@"\[([0-9]+:*)+ INFO\]: ([A-Za-z0-9_]+)\[.*\] logged in with entity id [0-9]+ at \(.*\)$");

        private Regex playerLeave =
            new Regex(@"\[([0-9]+:*)+\] \[Server thread/INFO\]: ([0-9a-zA-Z_]+) left the game$");
        private Regex playerLeavePaper =
            new Regex(@"\[([0-9]+:*)+ INFO\]: ([0-9a-zA-Z_]+) left the game$");

        private async void HandleConsoleWrite(string line, EntityViewModel entityViewModel)
        {
            if (entityViewModel is ServerViewModel viewModel)
            {
                switch (viewModel.Server.Version.Type)
                {
                    case ServerVersion.VersionType.Vanilla:
                        HandlePlayerJoinLeave(line, viewModel, playerJoin, playerLeave);
                        break;
                    case ServerVersion.VersionType.Paper:
                        HandlePlayerJoinLeave(line, viewModel, playerJoinPaper, playerLeavePaper);
                        HandlePlayerJoinLeave(line,viewModel, playerJoinPaper2, playerLeavePaper);
                        break;
                    default:
                        throw new Exception("Handle Player join/leave function does not implement "+viewModel.Server.Version.Type);
                } 
            }
        }

        private async void HandlePlayerJoinLeave(string line, ServerViewModel viewModel, Regex playerJoin, Regex playerLeave)
        {
            Match joinMatch = playerJoin.Match(line);
            if (joinMatch.Success)
            {
                string playerName = joinMatch.Groups[2].Value;
                try
                {
                    bool found = false;
                    foreach (ServerPlayer serverPlayer in viewModel.PlayerList)
                    {
                        if (serverPlayer.Player.Name.Equals(playerName))
                        {
                            serverPlayer.IsOnline = true;
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        Player p = await PlayerManager.Instance.GetPlayer(playerName);
                        ServerPlayer player = new ServerPlayer(p, viewModel, viewModel.OPList.Contains(p), true);
                        Application.Current.Dispatcher?.Invoke(() => viewModel.PlayerList.Add(player));
                    }
                    
                    viewModel.RefreshPlayerList();
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

                foreach (ServerPlayer serverPlayer in viewModel.PlayerList)
                {
                    if (serverPlayer.Player.Name.Equals(playerName))
                    {
                        serverPlayer.IsOnline = false;
                    }
                }
                
                viewModel.RefreshPlayerList();
            }
        }
    }
}