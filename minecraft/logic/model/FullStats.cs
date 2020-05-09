using System.Collections.Generic;
using System.Linq;
using fork.logic.Utils;

namespace fork.Logic.Model
{
    public class FullStats
    {
        public string Motd { get; }
        public string GameMode { get; }
        public string MapName { get; }
        public string HostIP { get; }
        public string GameID { get; }
        public string Version{ get; }
        public string ServerInfo { get; set; }
        public int OnlinePlayers{ get; }
        public int MaxPlayers{ get; }
        public int HostPort{ get; }
        public List<string> PlayerList{ get; set; }
        public List<string> PluginList{ get; set; }
        
        //public FullStats() {}

        public FullStats(byte[] data)
        {
            List<string> datos = ByteUtils.SplitByteArray(data);

            Motd = datos[3];
            GameMode = datos[5];
            GameID = datos[7];
            Version = datos[9];
            MapName = datos[13];
            OnlinePlayers = int.Parse(datos[15]);
            MaxPlayers = int.Parse(datos[17]);
            HostPort = int.Parse(datos[19]);
            HostIP = datos[21];

            SetPluginList(datos[11]);
            SetPlayerList(datos);
        }
        
        private void SetPlayerList(List<string> players)
        {
            PlayerList = new List<string>();

            for (int i = 25; i < players.Count; i++)
            {
                PlayerList.Add(players[i]);
            }
        }

        private void SetPluginList(string pluginInfo)
        {
            PluginList = new List<string>();
            if (string.IsNullOrEmpty(pluginInfo))
            {
                ServerInfo = "Vanilla";
            }
            else
            {
                string[] infoPlugins = pluginInfo.Split(':');
                string[] plugins = infoPlugins.Last().Split(';');

                ServerInfo = infoPlugins.First();
                
                foreach (string plugin in plugins)
                {
                    PluginList.Add(plugin.Trim());
                }
            }
        }
    }
}