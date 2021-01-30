using System;
using Fork.Logic.Model.Settings;

namespace Fork.Logic.Model.ProxyModels
{
    public interface NetworkServer : IComparable<NetworkServer>
    {
        string Name { get; set; }
        string Motd { get; set; }
        string Address { get; set; }
        bool Restricted { get; set; }
        bool IsForkServer { get; }
        Settings.Server ProxyServer { get; }
    }
}