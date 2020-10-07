using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Fork.Logic.Persistence;
using Newtonsoft.Json;

namespace Fork.Logic.Model
{
    [Serializable]
    public class Server : Entity
    {
        private ServerSettings serverSettings;
        
        public string UID { get; set; }
        public string Name { get; set; }
        public ServerVersion Version { get; set; }
        public JavaSettings JavaSettings { get; set; }

        public bool Initialized { get; set; } = false;
        
        public ServerRestart Restart1 { get; set; }
        public ServerRestart Restart2 { get; set; }
        public ServerRestart Restart3 { get; set; }
        public ServerRestart Restart4 { get; set; }
        [JsonIgnore]
        public ServerSettings ServerSettings {
            get
            {
                if (serverSettings == null)
                    serverSettings = new ServerSettings(new FileReader().ReadServerSettings(Path.Combine(App.ServerPath,Name)));
                return serverSettings;
            }
            set => serverSettings = value;
        }

        [JsonIgnore]
        public string FullName => Name + " (" + Version.Version + ")";

        [JsonIgnore]
        public string JarLink => Version.JarLink;

        public Server(String name, ServerVersion version, ServerSettings serverSettings, JavaSettings javaSettings) {
            Name = name;
            Version = version;
            JavaSettings = javaSettings;
            ServerSettings = serverSettings;
            Restart1 = new ServerRestart(false,new SimpleTime(0,0));
            Restart2 = new ServerRestart(false,new SimpleTime(6,0));
            Restart3 = new ServerRestart(false,new SimpleTime(12,0));
            Restart4 = new ServerRestart(false,new SimpleTime(18,0));
            UID = Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// Constructor for deserializer
        /// </summary>
        public Server() {}

        [OnDeserialized]
        internal void EnsureUid(StreamingContext context)
        {
            if (UID == null ||UID.Equals(""))
            {
                UID = Guid.NewGuid().ToString("D");
            }
        }

        public override string ToString()
        {
            string name = Name;
            if (Name.Length>10)
            {
                name = name.Substring(0, 10);
            }
            return name + " ("+Version.Version+")";
        }
    }
}
