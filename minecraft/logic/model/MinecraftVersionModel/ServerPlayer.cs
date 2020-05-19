using fork.ViewModel;

namespace fork.Logic.Model.MinecraftVersionPojo
{
    public class ServerPlayer
    {
        public Player Player { get; set; }
        public ServerViewModel ServerViewModel { get; set; }
        public bool IsOP { get; set; }

        public ServerPlayer(Player player, ServerViewModel viewModel, bool isOp)
        {
            Player = player;
            ServerViewModel = viewModel;
            IsOP = isOp;
        }
    }
}