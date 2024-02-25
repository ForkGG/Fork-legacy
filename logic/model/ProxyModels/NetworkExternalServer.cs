using System;

namespace Fork.Logic.Model.ProxyModels;

public class NetworkExternalServer : NetworkServer
{
    public NetworkExternalServer(Settings.Server server, string name)
    {
        Name = name;
        Motd = server.motd;
        Address = server.address;
        Restricted = server.restricted;
    }

    public string Name { get; set; }
    public string Motd { get; set; }
    public string Address { get; set; }
    public bool Restricted { get; set; }
    public bool IsForkServer => false;

    public Settings.Server ProxyServer => new()
        { address = Address, motd = Motd, restricted = Restricted, ForkServer = IsForkServer };

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