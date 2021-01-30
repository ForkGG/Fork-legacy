using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Fork.Logic.Model.Settings
{
    public enum TabListType
    {
        GLOBAL_PING,
        GLOBAL,
        SERVER
    }

    public class Server
    {
        public string motd { get; set; } = "Default MOTD";
        public string address { get; set; } = "0.0.0.0:25565";
        public bool restricted { get; set; } = false;
        public bool ForkServer { get; set; } = false;
        public string ForkServerUid { get; set; } = "";
    }

    public class Listener
    {
        public int query_port { get; set; } = 25565;
        public int max_players { get; set; } = 1;
        public int tab_size { get; set; } = 60;
        public string motd { get; set; } = "Bungee Server hosted by Fork.gg";

        [YamlIgnore]
        public string motdUnescaped
        {
            get => motd.Replace("\\n", "\n");
            set => motd = value.Replace("\n", "\\n").Replace("\r", "");
        }

        public TabListType tab_list { get; set; } = TabListType.GLOBAL_PING;
        public string host { get; set; } = "0.0.0.0:25565";
        public bool ping_passthrough { get; set; } = false;
        public bool force_default_server { get; set; } = false;
        public bool proxy_protocol { get; set; } = false;
        public bool query_enabled { get; set; } = false;
        public bool bind_local_address { get; set; } = true;
        public List<string> priorities { get; set; } = new();
        public Dictionary<string, string> forced_hosts { get; set; } = new();
    }

    public class BungeeSettings
    {
        [YamlIgnore]
        public List<TabListType> TabListTypes { get; } = new(Enum.GetValues(typeof(TabListType)).Cast<TabListType>());

        public int server_connect_timeout { get; set; } = 5000;
        public int player_limit { get; set; } = -1;
        public int network_compression_threshold { get; set; } = 256;
        public int connection_throttle { get; set; } = 4000;
        public int connection_throttle_limit { get; set; } = 3;
        public int timeout { get; set; } = 30000;
        public int remote_ping_cache { get; set; } = -1;
        public int remote_ping_timeout { get; set; } = 5000;
        public bool ip_forward { get; set; } = true;
        public bool log_commands { get; set; } = false;
        public bool online_mode { get; set; } = true;
        public bool forge_support { get; set; } = false;
        public bool inject_commands { get; set; } = false;
        public bool log_pings { get; set; } = true;

        public bool prevent_proxy_connections { get; set; } = false;

        //public string stats { get; set; }
        public Dictionary<string, List<string>> permissions { get; set; } = new();
        public Dictionary<string, Server> servers { get; set; } = new();
        public List<Listener> listeners { get; set; } = new();
        public List<string> disabled_commands { get; set; } = new();
        public Dictionary<string, List<string>> groups { get; set; } = new();
    }
}