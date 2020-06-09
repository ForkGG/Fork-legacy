using System;
using fork.ViewModel;
using Server = fork.Logic.Model.Settings.Server;

namespace Fork.Logic.Model.ProxyModels
{
    public class NetworkForkServer : NetworkServer
    {
        private string motdEscaped;
        
        public string Name { get; set; }

        public string Motd
        {
            get { return motdEscaped.Replace("\\n", "\n"); }
            set { motdEscaped = value.Replace("\n", "\\n").Replace("\r", ""); }
        }

        public string Address { get; set; }
        public bool Restricted { get; set; }

        public Server ProxyServer => new Server {address = Address, motd = motdEscaped, restricted = Restricted, forkServer = IsForkServer, forkServerUid = ServerViewModel.Server.UID};
        public bool IsForkServer => true;

        public ServerViewModel ServerViewModel { get; set; }

        public NetworkForkServer(ServerViewModel serverViewModel, Server server, string name)
        {
            ServerViewModel = serverViewModel;
            Name = name;
            Motd = server.motd;
            Address = server.address;
            Restricted = server.restricted;
        }

        public int CompareTo(NetworkServer otherNetworkServer)
        {
            int isForkServerCompare = otherNetworkServer.IsForkServer.CompareTo(IsForkServer);
            if (isForkServerCompare != 0)
            {
                return isForkServerCompare;
            }

            return string.Compare(Name, otherNetworkServer.Name, StringComparison.Ordinal);
        }
    }
}