using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Fork.Logic.CustomConsole;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.EventArgs;
using Fork.Logic.Model.ServerConsole;
using Fork.Logic.Persistence;
using Fork.Logic.Utils;
using Fork.ViewModel;
using Websocket.Client;
using Websocket.Client.Models;

namespace Fork.Logic.WebRequesters
{
    public class WebSocketHandler : IDisposable
    {
#if DEBUG
        private readonly string discordUrl = "ws://localhost:8181";
#else
        private readonly string discordUrl = "wss://fork.gg:8181";
#endif
        private WebsocketClient discordWebSocket;
        private readonly ManualResetEvent exitEvent = new(false);

        public WebSocketState DiscordWebSocketState { get; private set; }

        public async Task SetupDiscordWebSocket()
        {
            Func<ClientWebSocket> factory = () => new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(120),
                    Credentials = CredentialCache.DefaultNetworkCredentials,
                    UseDefaultCredentials = true
                }
            };
            using (discordWebSocket = new WebsocketClient(new Uri(discordUrl), factory))
            {
                discordWebSocket.IsReconnectionEnabled = true;
                discordWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
                discordWebSocket.ReconnectTimeout = null; //TimeSpan.FromSeconds(60);
                discordWebSocket.MessageReceived.Subscribe(HandleDiscordWebSocketMessage);
                discordWebSocket.ReconnectionHappened.Subscribe(HandleDiscordWebSocketReconnection);
                discordWebSocket.DisconnectionHappened.Subscribe(HandleDiscordWebSocketDisconnection);
                discordWebSocket.Start();
                Console.WriteLine("Started WebSocket Client for Discord bot");
                exitEvent.WaitOne();
            }
        }

        private void HandleDiscordWebSocketMessage(ResponseMessage message)
        {
            Task.Run(() =>
            {
                try
                {
                    HandleDiscordSocketMessageAsync(message);
                }
                catch (Exception e)
                {
                    ErrorLogger.Append(e);
                }
            });
        }

        private void HandleDiscordWebSocketReconnection(ReconnectionInfo reconnectionInfo)
        {
            ApplicationManager.Instance.MainViewModel.AppSettingsViewModel.UpdateDiscordWebSocketState(reconnectionInfo
                .Type);
            DiscordBotLogin();
        }

        private void HandleDiscordWebSocketDisconnection(DisconnectionInfo disconnectionInfo)
        {
            ApplicationManager.Instance.MainViewModel.AppSettingsViewModel.UpdateDiscordWebSocketState(disconnectionInfo
                .Type);
        }

        private void DiscordBotLogin()
        {
            try
            {
                if (discordWebSocket.IsRunning)
                    discordWebSocket.Send("login|" + AppSettingsSerializer.Instance.AppSettings.DiscordBotToken);
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        }

        /// <summary>
        ///     Handles a received message, by performing actions and returning the answer
        /// </summary>
        /// <param name="message">Received message</param>
        /// <returns>Answer to respond to the Server</returns>
        private void HandleDiscordSocketMessageAsync(ResponseMessage message)
        {
            if (message.MessageType != WebSocketMessageType.Text) Task.Run(() => SendMessageAsync("40"));

            Console.WriteLine("Received message via the Discord WebSocket: " + message.Text);
            string[] splitted = message.Text.Split('|');
            switch (splitted[0])
            {
                case "status":
                    if (splitted.Length > 2)
                        ApplicationManager.Instance.MainViewModel.AppSettingsViewModel.DiscordLinkStatusUpdate(
                            splitted[1], splitted[2]);
                    else
                        ApplicationManager.Instance.MainViewModel.AppSettingsViewModel.DiscordLinkStatusUpdate(
                            splitted[1]);
                    break;
                case "rec":
                    RecreateToken();
                    break;
                case "stop":
                    Task.Run(() => SendMessageAsync(BuildResponseString(splitted, StopServer(splitted))));
                    break;
                case "start":
                    Task.Run(() => SendMessageAsync(BuildResponseString(splitted, StartServerAsync(splitted))));
                    break;
                case "subscribe":
                    SubscribeToEvent(splitted[1]);
                    break;
                case "unsub":
                    UnsubscribeFromEvent(splitted[1]);
                    break;
                case "playerList":
                    SendPlayerList(splitted[1]);
                    break;
                default:
                    Task.Run(() => SendMessageAsync("43|" + message.Text));
                    break;
            }
        }

        private void RecreateToken()
        {
            AppSettingsSerializer.Instance.AppSettings.DiscordBotToken = TokenUtils.GenerateDiscordToken();
            AppSettingsSerializer.Instance.SaveSettings();
            DiscordBotLogin();
        }

        /// <summary>
        ///     Sends a message to the server
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendMessageAsync(string message)
        {
            Console.WriteLine("Sending message to Discord bot via WebSocket: " + message);
            int retries = 0;
            while (!discordWebSocket.IsRunning && retries < 10)
            {
                retries++;
                await Task.Delay(3_000);
            }

            if (!discordWebSocket.IsRunning) return;

            discordWebSocket.Send(message);
        }

        /// <summary>
        ///     Build a response for the server depending on a status code of a request
        /// </summary>
        /// <param name="splittedMessage"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private string BuildResponseString(string[] splittedMessage, int status)
        {
            List<string> resultSplitted = new();
            resultSplitted.Add("notify");
            switch (splittedMessage[0])
            {
                //notify|{servername}|{discordname}|{channelid}|{messageid}|stop|{status}
                case "stop":
                    resultSplitted.Add(splittedMessage[1]);
                    resultSplitted.Add(splittedMessage[2]);
                    resultSplitted.Add(splittedMessage[3]);
                    resultSplitted.Add(splittedMessage[4]);
                    resultSplitted.Add(splittedMessage[0]);
                    resultSplitted.Add(status.ToString());
                    break;
                //notify|{servername}|{discordname}|{channelid}|{messageid}|start|{status}
                case "start":
                    resultSplitted.Add(splittedMessage[1]);
                    resultSplitted.Add(splittedMessage[2]);
                    resultSplitted.Add(splittedMessage[3]);
                    resultSplitted.Add(splittedMessage[4]);
                    resultSplitted.Add(splittedMessage[0]);
                    resultSplitted.Add(status.ToString());
                    break;

                //Default case should not happen
                default:
                    return "43|" + string.Join('|', splittedMessage);
            }

            return string.Join('|', resultSplitted);
        }

        /// <summary>
        ///     Stop a server after the according request was received
        /// </summary>
        /// <param name="splitted">Request message split</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private int StopServer(string[] splitted)
        {
            string serverName = splitted[1];
            List<EntityViewModel> targets = ServerManager.Instance.Entities
                .Where(entity => entity.Name.ToLower().Equals(serverName.ToLower())).ToList();
            if (targets.Count < 1) return 404;

            if (targets[0].CurrentStatus == ServerStatus.STOPPED) return 400;

            ConsoleWriter.Write(
                new ConsoleMessage($"Discord user {splitted[2]} stopped server remotely",
                    ConsoleMessage.MessageLevel.WARN), targets[0]);
            if (targets[0] is ServerViewModel serverViewModel)
                ServerManager.Instance.StopServer(serverViewModel);
            else if (targets[0] is NetworkViewModel networkViewModel)
                Task.Run(() => ServerManager.Instance.StopNetworkAsync(networkViewModel));
            else
                throw new NotImplementedException();

            return 200;
        }

        /// <summary>
        ///     Start a server after the according request was received
        /// </summary>
        /// <param name="splitted">Request message split</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private int StartServerAsync(string[] splitted)
        {
            string serverName = splitted[1];
            List<EntityViewModel> targets = ServerManager.Instance.Entities
                .Where(entity => entity.Name.ToLower().Equals(serverName.ToLower())).ToList();
            if (targets.Count < 1) return 44;

            if (targets[0].CurrentStatus != ServerStatus.STOPPED) return 40;

            ConsoleWriter.Write(
                new ConsoleMessage($"Discord user {splitted[2]} started server remotely",
                    ConsoleMessage.MessageLevel.WARN), targets[0]);
            Task.Run(async () =>
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Delay(2000));
                if (targets[0] is ServerViewModel serverViewModel)
                    tasks.Add(ServerManager.Instance.StartServerAsync(serverViewModel));
                else if (targets[0] is NetworkViewModel networkViewModel)
                    tasks.Add(ServerManager.Instance.StartNetworkAsync(networkViewModel));
                else
                    throw new NotImplementedException();

                //Wait at least 2 seconds, to ensure the right order of messages
                await Task.WhenAll(tasks);
                await Task.Run(() => SendMessageAsync(BuildResponseString(splitted, 21)));
            });

            return 20;
        }

        /// <summary>
        ///     Subscribe to the requested Event. This will start sending messages of the according event to the server
        /// </summary>
        /// <param name="eventName"></param>
        private void SubscribeToEvent(string eventName)
        {
            switch (eventName)
            {
                case "playerEvent":
                    SubscribeToPlayerEvent();
                    break;
                case "serverListEvent":
                    SubscribeToServerListEvent();
                    break;
                default:
                    Task.Run(() => SendMessageAsync($"43|{eventName}"));
                    break;
            }
        }

        /// <summary>
        ///     Unsubscribes from an Event. This will stop sending messages about the Event to the WebSocket server
        /// </summary>
        /// <param name="eventName"></param>
        private void UnsubscribeFromEvent(string eventName)
        {
            switch (eventName)
            {
                case "playerEvent":
                    UnsubscribeFromPlayerEvent();
                    break;
                case "serverListEvent":
                    UnsubscribeFromServerListEvent();
                    break;
                default:
                    Task.Run(() => SendMessageAsync($"43|{eventName}"));
                    break;
            }
        }

        private void SubscribeToPlayerEvent()
        {
            ApplicationManager.Instance.PlayerEvent -= HandlePlayerEvent;
            ApplicationManager.Instance.PlayerEvent += HandlePlayerEvent;
        }

        private void UnsubscribeFromPlayerEvent()
        {
            ApplicationManager.Instance.PlayerEvent -= HandlePlayerEvent;
        }

        private void SubscribeToServerListEvent()
        {
            ApplicationManager.Instance.ServerListEvent -= HandleServerListEvent;
            ApplicationManager.Instance.ServerListEvent += HandleServerListEvent;
            ApplicationManager.Instance.PlayerEvent -= HandleServerListEvent;
            ApplicationManager.Instance.PlayerEvent += HandleServerListEvent;
            SendServerList();
        }

        private void UnsubscribeFromServerListEvent()
        {
            ApplicationManager.Instance.ServerListEvent -= HandleServerListEvent;
            ApplicationManager.Instance.PlayerEvent -= HandleServerListEvent;
        }

        private void HandlePlayerEvent(object sender, PlayerEventArgs e)
        {
            string type = e.EventType == PlayerEventArgs.PlayerEventType.Join ? "pjoin" : "pleave";

            Task.Run(() => SendMessageAsync($"event|{type}|{e.Server.Name}|{e.PlayerName}"));
        }

        private void HandleServerListEvent(object sender, EventArgs e)
        {
            SendServerList();
        }

        /// <summary>
        ///     Send a list of all servers to the WebSocket server
        /// </summary>
        private void SendServerList()
        {
            List<EntityViewModel> viewModels = new List<EntityViewModel>(ServerManager.Instance.Entities);
            List<string> resultList = new List<string>(2 + viewModels.Capacity * 6);
            resultList.Add("event");
            resultList.Add("serverList");
            foreach (EntityViewModel viewModel in viewModels)
            {
                resultList.Add(viewModel.Name);
                resultList.Add(viewModel.Entity.Version.Type.ToString());
                resultList.Add(viewModel.Entity.Version.Version);
                resultList.Add(viewModel.CurrentStatus.ToString());
                if (viewModel is ServerViewModel serverViewModel)
                {
                    resultList.Add(serverViewModel.PlayerList.Count(player => player.IsOnline).ToString());
                    resultList.Add(serverViewModel.Server.ServerSettings.MaxPlayers.ToString());
                }
                else if (viewModel is NetworkViewModel networkViewModel)
                {
                    //TODO add a way to check how many players are connected to network
                    resultList.Add("0");
                    resultList.Add(networkViewModel.Network.Config.player_limit.ToString());
                }
                else
                {
                    resultList.Add("0");
                    resultList.Add("0");
                }
            }

            Task.Run(() => SendMessageAsync(string.Join('|', resultList)));
        }

        /// <summary>
        ///     Send a list of all players of a given server to the WebSocket server
        /// </summary>
        /// <param name="serverName"></param>
        private void SendPlayerList(string serverName)
        {
            List<string> resultList = new List<string>();
            resultList.Add("playerList");
            resultList.Add(serverName);

            EntityViewModel entityViewModel = ServerManager.Instance.Entities.First(
                entity => entity is ServerViewModel && entity.Name.ToLower().Equals(serverName.ToLower()));
            if (entityViewModel is ServerViewModel serverViewModel)
                foreach (ServerPlayer serverPlayer in serverViewModel.PlayerList.Where(player => player.IsOnline))
                    resultList.Add(serverPlayer.Player.Name);

            Task.Run(() => SendMessageAsync(string.Join('|', resultList)));
        }

        public void Dispose()
        {
            ApplicationManager.Instance.PlayerEvent -= HandlePlayerEvent;
            ApplicationManager.Instance.ServerListEvent -= HandleServerListEvent;
            ApplicationManager.Instance.PlayerEvent -= HandleServerListEvent;
            discordWebSocket?.Dispose();
            exitEvent?.Dispose();
        }
    }
}