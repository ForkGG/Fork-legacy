using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using fork.Logic.Model.Settings;
using fork.Logic.Persistence.YMLReaders;
using fork.Logic.WebRequesters;
using Newtonsoft.Json;

namespace fork.Logic.Model.ProxyModels
{
    [Serializable]
    public class Network : Entity
    {
        private BungeeSettingsSerializer configSerializer;

        public string UID { get; set; }
        public string Name { get; set; }
        public ServerVersion.VersionType ProxyType { get; set; }
        public JavaSettings JavaSettings { get; set; }
        public bool SyncServers { get; set; } = false;

        public bool Initialized { get; set; } = false;
        public ServerVersion Version { get; set; }
        [JsonIgnore] public BungeeSettings Config { get; set; }

        [JsonIgnore]
        public int Port
        {
            get => int.Parse(Config.listeners[0].host.Split(':').Last());
            set
            {
                string address = Config.listeners[0].host;
                string oldPort = address.Split(':').Last();
                address = address.Replace(oldPort, value.ToString());
                Config.listeners[0].host = address;
            }
        }

        public Network(string name, ServerVersion.VersionType proxyType, JavaSettings javaSettings,
            ServerVersion version)
        {
            Name = name;
            ProxyType = proxyType;
            JavaSettings = javaSettings;
            Version = version;
            UID = Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// Constructor for deserializer
        /// </summary>
        public Network()
        {
        }

        [OnDeserialized]
        internal void EnsureUid(StreamingContext context)
        {
            if (UID == null || UID.Equals(""))
            {
                UID = Guid.NewGuid().ToString("D");
            }
        }

        public void ReadSettings()
        {
            if (configSerializer == null)
            {
                configSerializer =
                    new BungeeSettingsSerializer(new FileInfo(Path.Combine(App.ApplicationPath, Name, "config.yml")));
            }

            Config = configSerializer.ReadSettings();
        }

        public void WriteSettings()
        {
            if (configSerializer == null)
            {
                configSerializer =
                    new BungeeSettingsSerializer(new FileInfo(Path.Combine(App.ApplicationPath, Name, "config.yml")));
            }

            configSerializer.WriteSettings(Config);
        }

        public override string ToString()
        {
            string name = Name;
            if (Name.Length > 10)
            {
                name = name.Substring(0, 10);
            }

            return name + " (" + ProxyType + ")";
        }
    }
}