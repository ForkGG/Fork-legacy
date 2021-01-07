using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Fork.Logic.CustomConsole;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.ServerConsole;
using Fork.Logic.Persistence;
using Fork.ViewModel;
using Microsoft.VisualBasic;
using Websocket.Client;
using Websocket.Client.Logging;
using Websocket.Client.Models;

namespace Fork.Logic.WebRequesters
{
    public class WebSocketHandler
    {
#if DEBUG
        private readonly string discordUrl = "ws://localhost:8181";
#else
        //TODO change this to actual ip
        private readonly string discordUrl = "ws://fork.gg:8181";
#endif
        private WebsocketClient discordWebSocket;
        private ManualResetEvent exitEvent = new ManualResetEvent(false);


        public WebSocketState DiscordWebSocketState { get; private set; }

        public WebSocketHandler()
        {
        }

        public async Task SetupDiscordWebSocket()
        {
            Func<ClientWebSocket> factory = () => new ClientWebSocket()
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(120)
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
                exitEvent.WaitOne();
                //await Task.Delay(20000);
                //await discordWebSocket.Stop(WebSocketCloseStatus.NormalClosure, "Fork shutting down");
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
                {
                    discordWebSocket.Send("login|" + AppSettingsSerializer.Instance.AppSettings.DiscordBotToken);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        }

        /// <summary>
        /// Handles a received message, by performing actions and returning the answer
        /// </summary>
        /// <param name="message">Received message</param>
        /// <returns>Answer to respond to the Server</returns>
        private void HandleDiscordSocketMessageAsync(ResponseMessage message)
        {
            if (message.MessageType != WebSocketMessageType.Text)
            {
                Task.Run(() => SendMessageAsync("400"));
            }

            string[] splitted = message.Text.Split('|');
            switch (splitted[0])
            {
                case "stop":
                    Task.Run(() => SendMessageAsync(BuildResponseString(splitted, StopServer(splitted))));
                    break;
                case "start":
                    Task.Run(() => SendMessageAsync(BuildResponseString(splitted, StartServerAsync(splitted))));
                    break;
                default:
                    Task.Run(() => SendMessageAsync("403|"+message.Text));
                    break;
            }
        }

        private async Task SendMessageAsync(string message)
        {
            int retries = 0;
            while (!discordWebSocket.IsRunning && retries < 10)
            {
                retries++;
                await Task.Delay(30_000);
            }

            if (!discordWebSocket.IsRunning)
            {
                return;
            }

            discordWebSocket.Send(message);
        }

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
                    return "403|" + string.Join('|', splittedMessage);
            }

            return string.Join('|', resultSplitted);
        }
        
        private int StopServer(string[] splitted)
        {
            string serverName = splitted[1];
            List<EntityViewModel> targets = ServerManager.Instance.Entities
                .Where(entity => entity.Name.ToLower().Equals(serverName.ToLower())).ToList();
            if (targets.Count < 1)
            {
                return 404;
            }
            
            if (targets[0].CurrentStatus == ServerStatus.STOPPED)
            {
                return 400;
            }
            
            ConsoleWriter.Write(new ConsoleMessage($"Discord user {splitted[2]} stopped server remotely",ConsoleMessage.MessageLevel.WARN), targets[0]);
            if (targets[0] is ServerViewModel serverViewModel)
            {
                ServerManager.Instance.StopServer(serverViewModel);
            } else if (targets[0] is NetworkViewModel networkViewModel)
            {
                Task.Run(() => ServerManager.Instance.StopNetworkAsync(networkViewModel));
            }
            else
            {
                throw new NotImplementedException();
            }

            return 200;
        }

        private int StartServerAsync(string[] splitted)
        {
            string serverName = splitted[1];
            List<EntityViewModel> targets = ServerManager.Instance.Entities
                .Where(entity => entity.Name.ToLower().Equals(serverName.ToLower())).ToList();
            if (targets.Count < 1)
            {
                return 404;
            }

            if (targets[0].CurrentStatus != ServerStatus.STOPPED)
            {
                return 400;
            }

            ConsoleWriter.Write(new ConsoleMessage($"Discord user {splitted[2]} started server remotely",ConsoleMessage.MessageLevel.WARN), targets[0]);
            Task.Run(async () =>
            {
                if (targets[0] is ServerViewModel serverViewModel)
                {
                    await ServerManager.Instance.StartServerAsync(serverViewModel);
                } else if (targets[0] is NetworkViewModel networkViewModel)
                {
                    await ServerManager.Instance.StartNetworkAsync(networkViewModel);
                }
                else
                {
                    throw new NotImplementedException();
                }
                Task.Run(() => SendMessageAsync(BuildResponseString(splitted, 201)));
            });

            return 200;
        }
    }
}