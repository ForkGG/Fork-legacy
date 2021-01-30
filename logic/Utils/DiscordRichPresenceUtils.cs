using System.Linq;
using DiscordRPC;
using DiscordRPC.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.ViewModel;

namespace Fork.Logic.Utils
{
    public static class DiscordRichPresenceUtils
    {
        private static DiscordRpcClient rpcClient;

        public static void SetupRichPresence()
        {
            rpcClient = new DiscordRpcClient("795015105061847111");
            rpcClient.Logger = new ConsoleLogger(LogLevel.Error);
            rpcClient.Initialize();

            UpdateRichPresence();
            foreach (EntityViewModel entityViewModel in ServerManager.Instance.Entities)
                entityViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != null && args.PropertyName.Equals(nameof(entityViewModel.CurrentStatus)))
                        UpdateRichPresence();
                };
        }

        public static void UpdateRichPresence()
        {
            int runningCount =
                ServerManager.Instance.Entities.Count(entity => entity.CurrentStatus == ServerStatus.RUNNING);
            string state = "";
            switch (runningCount)
            {
                case 0:
                    state = "";
                    break;
                case 1:
                    state = "Running 1 Server";
                    break;
                default:
                    state = $"Running {runningCount} Servers";
                    break;
            }

            rpcClient.SetPresence(new RichPresence
            {
                Details = "Hosting Minecraft Servers",
                State = state,
                Assets = new Assets
                {
                    LargeImageKey = "fork_large", //TODO
                    LargeImageText = "fork.gg"
                }
            });
        }
    }
}