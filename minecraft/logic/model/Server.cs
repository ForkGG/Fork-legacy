using System;
using System.IO;
using System.Xml.Serialization;
using nihilus.Logic.Persistence;

namespace nihilus.Logic.Model
{
    [Serializable]
    public class Server
    {
        private ServerSettings serverSettings;
        
        public String Name { get; set; }
        public ServerVersion Version { get; set; }
        public ServerJavaSettings JavaSettings { get; set; }
        
        public ServerRestart Restart1 { get; set; }
        public ServerRestart Restart2 { get; set; }
        public ServerRestart Restart3 { get; set; }
        public ServerRestart Restart4 { get; set; }
        [XmlIgnore]
        public ServerSettings ServerSettings {
            get
            {
                if (serverSettings == null)
                    serverSettings = new ServerSettings(new FileReader().ReadServerSettings(Path.Combine(App.ApplicationPath,Name)));
                return serverSettings;
            }
            set => serverSettings = value;
        }

        public Server(String name, ServerVersion version, ServerSettings serverSettings, ServerJavaSettings javaSettings) {
            Name = name;
            Version = version;
            JavaSettings = javaSettings;
            ServerSettings = serverSettings;
            Restart1 = new ServerRestart(false,new SimpleTime(0,0));
            Restart2 = new ServerRestart(false,new SimpleTime(6,0));
            Restart3 = new ServerRestart(false,new SimpleTime(12,0));
            Restart4 = new ServerRestart(false,new SimpleTime(18,0));

        }

        /// <summary>
        /// Constructor for xml deserializer
        /// </summary>
        public Server() {}

        public override string ToString()
        {
            string name = Name;
            if (Name.Length>10)
            {
                name = name.Substring(0, 9);
            }
            return name + " ("+Version.Version+")";
        }
    }
}
