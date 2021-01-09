using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Fork.Logic.CustomConsole;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.EventArgs;
using Fork.Logic.Model.ServerConsole;
using Fork.Logic.Persistence;
using Fork.ViewModel;
using Microsoft.Win32.SafeHandles;
using Websocket.Client;
using Websocket.Client.Models;
using Timer = System.Timers.Timer;

namespace Fork.Logic.WebRequesters
{
   
    public class WebSocketHandler : IDisposable
    {
#if DEBUG
        static public void Initialize(bool alwaysCreateNewConsole = true)
        {
            bool consoleAttached = true;
            if (alwaysCreateNewConsole
                || (AttachConsole(ATTACH_PARRENT) == 0
                && Marshal.GetLastWin32Error() != ERROR_ACCESS_DENIED))
            {
                consoleAttached = AllocConsole() != 0;
            }

            if (consoleAttached)
            {
                InitializeOutStream();
                InitializeInStream();
            }
        }

        private static void InitializeOutStream()
        {
            var fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
            if (fs != null)
            {
                var writer = new StreamWriter(fs) { AutoFlush = true };
                Console.SetOut(writer);
                Console.SetError(writer);
            }
        }

        private static void InitializeInStream()
        {
            var fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
            if (fs != null)
            {
                Console.SetIn(new StreamReader(fs));
            }
        }

        private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode,
                                FileAccess dotNetFileAccess)
        {
            var file = new SafeFileHandle(CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
            if (!file.IsInvalid)
            {
                var fs = new FileStream(file, dotNetFileAccess);
                return fs;
            }
            return null;
        }

        #region Win API Functions and Constants
        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll",
            EntryPoint = "AttachConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern UInt32 AttachConsole(UInt32 dwProcessId);

        [DllImport("kernel32.dll",
            EntryPoint = "CreateFileW",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateFileW(
              string lpFileName,
              UInt32 dwDesiredAccess,
              UInt32 dwShareMode,
              IntPtr lpSecurityAttributes,
              UInt32 dwCreationDisposition,
              UInt32 dwFlagsAndAttributes,
              IntPtr hTemplateFile
            );

        private const UInt32 GENERIC_WRITE = 0x40000000;
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 FILE_SHARE_READ = 0x00000001;
        private const UInt32 FILE_SHARE_WRITE = 0x00000002;
        private const UInt32 OPEN_EXISTING = 0x00000003;
        private const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
        private const UInt32 ERROR_ACCESS_DENIED = 5;

        private const UInt32 ATTACH_PARRENT = 0xFFFFFFFF;

        #endregion
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
             await discordWebSocket.Start();
#if DEBUG
                Initialize();
                Console.WriteLine("Logging...");
#endif
                exitEvent.WaitOne();
            }
        }

        private void HandleDiscordWebSocketMessage(ResponseMessage message)
        {
            Task.Run(() =>
            {
                try
                {
#if DEBUG
                    Console.WriteLine("Message");
#endif
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
                Task.Run(() => SendMessageAsync("40"));
            }

            string[] splitted = message.Text.Split('|');
            switch (splitted[0])
            {
                case "status":
                    ApplicationManager.Instance.MainViewModel.AppSettingsViewModel.DiscordLinkStatusUpdate(splitted[1]);
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
                case "serverList":
                    SendServerList();
                    break;
                case "playerList":
                    SendPlayerList(splitted[1]);
                    break;
                default:
                    Task.Run(() => SendMessageAsync("43|"+message.Text));
                    break;
            }
        }

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Build a response for the server depending on a status code of a request
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
        /// Stop a server after the according request was received
        /// </summary>
        /// <param name="splitted">Request message split</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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

        /// <summary>
        /// Start a server after the according request was received
        /// </summary>
        /// <param name="splitted">Request message split</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private int StartServerAsync(string[] splitted)
        {
            string serverName = splitted[1];
            List<EntityViewModel> targets = ServerManager.Instance.Entities
                .Where(entity => entity.Name.ToLower().Equals(serverName.ToLower())).ToList();
            if (targets.Count < 1)
            {
                return 44;
            }

            if (targets[0].CurrentStatus != ServerStatus.STOPPED)
            {
                return 40;
            }

            ConsoleWriter.Write(new ConsoleMessage($"Discord user {splitted[2]} started server remotely",ConsoleMessage.MessageLevel.WARN), targets[0]);
            Task.Run(async () =>
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Delay(2000));
                if (targets[0] is ServerViewModel serverViewModel)
                {
                    tasks.Add(ServerManager.Instance.StartServerAsync(serverViewModel));
                } else if (targets[0] is NetworkViewModel networkViewModel)
                {
                    tasks.Add(ServerManager.Instance.StartNetworkAsync(networkViewModel));
                }
                else
                {
                    throw new NotImplementedException();
                }
                
                //Wait at least 2 seconds, to ensure the right order of messages
                await Task.WhenAll(tasks);
                await Task.Run(() => SendMessageAsync(BuildResponseString(splitted, 21)));
            });

            return 20;
        }

        /// <summary>
        /// Subscribe to the requested Event. This will start sending messages of the according event to the server
        /// </summary>
        /// <param name="eventName"></param>
        private void SubscribeToEvent(string eventName)
        {
            switch (eventName)
            {
                case "playerEvent":
                    SubscribeToPlayerEvent();
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

        private void HandlePlayerEvent(object sender, PlayerEventArgs e)
        {
            string type = e.EventType == PlayerEventArgs.PlayerEventType.Join ? "pjoin" : "pleave";

            Task.Run(() => SendMessageAsync($"event|{type}|{e.Server.Name}|{e.PlayerName}"));
        }

        /// <summary>
        /// Send a list of all servers to the WebSocket server
        /// </summary>
        private void SendServerList()
        {
            List<EntityViewModel> viewModels = new List<EntityViewModel>(ServerManager.Instance.Entities);
            List<string> resultList = new List<string>(2 + viewModels.Capacity * 6);
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
                }else
                {
                    resultList.Add("0");
                    resultList.Add("0");
                }
            }

            Task.Run(() => SendMessageAsync(string.Join('|', resultList)));
        }

        /// <summary>
        /// Send a list of all players of a given server to the WebSocket server
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
            {
                foreach (ServerPlayer serverPlayer in serverViewModel.PlayerList.Where(player => player.IsOnline))
                {
                    resultList.Add(serverPlayer.Player.Name);
                }
            }

            Task.Run(() => SendMessageAsync(string.Join('|', resultList)));
        }

        public void Dispose()
        {
            ApplicationManager.Instance.PlayerEvent -= HandlePlayerEvent;
            discordWebSocket?.Dispose();
            exitEvent?.Dispose();
        }
    }
}