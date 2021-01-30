using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Fork.Logic.Model.Settings;
using Fork.Logic.Persistence.YMLReaders;
using Newtonsoft.Json;

namespace Fork.Logic.Model.ProxyModels
{
    [Serializable]
    public class Network : Entity
    {
        private BungeeSettingsSerializer configSerializer;

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
        ///     Constructor for deserializer
        /// </summary>
        public Network()
        {
        }

        public ServerVersion.VersionType ProxyType { get; set; }
        public bool SyncServers { get; set; } = false;
        [JsonIgnore] [NotMapped] public BungeeSettings Config { get; set; }

        [JsonIgnore]
        [NotMapped]
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

        [Key] public string UID { get; set; }

        public string Name { get; set; }
        public virtual JavaSettings JavaSettings { get; set; }

        public bool Initialized { get; set; } = false;
        public bool StartWithFork { get; set; } = false;
        public int ServerIconId { get; set; }

        public virtual ServerVersion Version { get; set; }

        public override string ToString()
        {
            string name = Name;
            if (Name.Length > 10) name = name.Substring(0, 10);

            return name + " (" + ProxyType + ")";
        }

        [OnDeserialized]
        internal void EnsureUid(StreamingContext context)
        {
            if (UID == null || UID.Equals("")) UID = Guid.NewGuid().ToString("D");
        }

        public void ReadSettings()
        {
            if (configSerializer == null)
                configSerializer =
                    new BungeeSettingsSerializer(new FileInfo(Path.Combine(App.ServerPath, Name, "config.yml")));

            Config = configSerializer.ReadSettings();
        }

        public void WriteSettings()
        {
            if (configSerializer == null)
                configSerializer =
                    new BungeeSettingsSerializer(new FileInfo(Path.Combine(App.ServerPath, Name, "config.yml")));

            configSerializer.WriteSettings(Config);
        }
    }
}