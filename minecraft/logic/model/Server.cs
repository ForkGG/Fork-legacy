using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using nihilus.logic.persistence;

namespace nihilus.logic.model
{
    [Serializable]
    public class Server
    {
        private ServerSettings serverSettings;
        
        public String Name { get; set; }
        public ServerVersion Version { get; set; }
        public ServerJavaSettings JavaSettings { get; set; }
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

        public Server(String name, ServerVersion version, ServerSettings serverSettings, ServerJavaSettings javaSettings) {
            Name = name;
            Version = version;
            JavaSettings = javaSettings;
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
