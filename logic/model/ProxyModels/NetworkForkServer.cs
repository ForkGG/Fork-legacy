using System;
using Fork.ViewModel;

namespace Fork.Logic.Model.ProxyModels;

public class NetworkForkServer : NetworkServer
{
    private string motdEscaped;

    public NetworkForkServer(ServerViewModel serverViewModel, Settings.Server server, string name)
    {
        ServerViewModel = serverViewModel;
        Name = name;
        Motd = server.motd;
        Address = server.address;
        Restricted = server.restricted;
    }

    public ServerViewModel ServerViewModel { get; set; }

    public string Name { get; set; }

    public string Motd
    {
        get => motdEscaped.Replace("\\n", "\n");
        set => motdEscaped = value.Replace("\n", "\\n").Replace("\r", "");
    }

    public string Address { get; set; }
    public bool Restricted { get; set; }

    public Settings.Server ProxyServer => new()
    {
        address = Address, motd = motdEscaped, restricted = Restricted, ForkServer = IsForkServer,
        ForkServerUid = ServerViewModel.Server.UID
    };

    public bool IsForkServer => true;

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