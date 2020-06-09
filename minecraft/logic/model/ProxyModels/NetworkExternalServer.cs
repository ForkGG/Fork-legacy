using System;
using fork.Logic.Model;
using Server = fork.Logic.Model.Settings.Server;

namespace Fork.Logic.Model.ProxyModels
{
    public class NetworkExternalServer : NetworkServer
    {
        public string Name { get; set; }
        public string Motd { get; set; }
        public string Address { get; set; }
        public bool Restricted { get; set; }
        public bool IsForkServer => false;
        
        public Server ProxyServer => new Server {address = Address, motd = Motd, restricted = Restricted, forkServer = IsForkServer};

        public NetworkExternalServer(Server server, string name)
        {
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