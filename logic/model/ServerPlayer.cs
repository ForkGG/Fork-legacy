using System;
using System.Collections.Generic;
using Fork.ViewModel;

namespace Fork.Logic.Model
{
    public class ServerPlayer : IComparable<ServerPlayer>, IEquatable<ServerPlayer>
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

        public bool Equals(ServerPlayer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Player, other.Player) && Equals(ServerViewModel, other.ServerViewModel) && IsOP == other.IsOP && IsOnline == other.IsOnline;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerPlayer) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Player, ServerViewModel, IsOP, IsOnline);
        }
    }
}