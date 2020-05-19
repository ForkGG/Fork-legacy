using System;
using System.Collections.Generic;
using fork.ViewModel;

namespace fork.Logic.Model
{
    public class ServerPlayer : IComparable<ServerPlayer>
    {
        public Player Player { get; set; }
        public ServerViewModel ServerViewModel { get; set; }
        public bool IsOP { get; set; }
        public bool IsOnline { get; set; }

        public ServerPlayer(Player player, ServerViewModel viewModel, bool isOp, bool isOnline)
        {
            Player = player;
            ServerViewModel = viewModel;
            IsOP = isOp;
            IsOnline = isOnline;
        }


        public int CompareTo(ServerPlayer other)
        {
            int onlineCompare = other.IsOnline.CompareTo(IsOnline);
            if (onlineCompare != 0)
            {
                return onlineCompare;
            }

            int opCompare = other.IsOP.CompareTo(IsOP);
            if (opCompare != 0)
            {
                return opCompare;
            }

            return string.Compare(Player.Name, other.Player.Name, StringComparison.Ordinal);
        }
    }
}