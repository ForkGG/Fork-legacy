using System;
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
        [XmlIgnore]
        public ServerSettings ServerSettings {
            get
            {
                if (serverSettings == null)
                    serverSettings = new ServerSettings(new FileReader().ReadServerSettings(Name));
                return serverSettings;
            }
            set => serverSettings = value;
        }

        public Server(String name, ServerVersion version, ServerSettings serverSettings) {
            Name = name;
            Version = version;
            ServerSettings = serverSettings;
        }

        /// <summary>
        /// Constructor for xml deserializer
        /// </summary>
        public Server() {}

        public override string ToString()
        {
            return Name + "("+Version.Version+")";
        }
    }
}
