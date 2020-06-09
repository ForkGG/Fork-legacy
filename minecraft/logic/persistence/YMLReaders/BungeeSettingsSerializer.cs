using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using fork.Annotations;
using fork.Logic.Model;
using fork.Logic.Model.Settings;
using fork.ViewModel;
using YamlDotNet.Serialization;
using Server = fork.Logic.Model.Settings.Server;

namespace fork.Logic.Persistence.YMLReaders
{
    public class BungeeSettingsSerializer
    {
        public FileInfo ConfigYML { get; }

        public BungeeSettingsSerializer(FileInfo configYml)
        {
            ConfigYML = configYml;
        }

        public BungeeSettings ReadSettings()
        {
            if (!ConfigYML.Exists)
            {
                InitializeBungeeConfig();
            }
            using (StreamReader streamReader = new StreamReader(ConfigYML.OpenRead()))
            {
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                return deserializer.Deserialize<BungeeSettings>(streamReader);
            }
        }

        public void WriteSettings(BungeeSettings settings)
        {
            using (StreamWriter streamWriter = new StreamWriter(ConfigYML.Create()))
            {
                Serializer serializer = new Serializer();
                serializer.Serialize(streamWriter, settings);
            }
        }

        private void InitializeBungeeConfig()
        {
            BungeeSettings bungeeSettings = new BungeeSettings();
            bungeeSettings.groups.Add("md_5",new List<string>{"admin"});
            bungeeSettings.disabled_commands.Add("disabledcommandhere");
            bungeeSettings.servers.Add("lobby",new Server());
            bungeeSettings.listeners = new List<Listener>{new Listener()};
            bungeeSettings.listeners[0].priorities.Add("lobby");
            bungeeSettings.permissions.Add("default",new List<string>{"bungeecord.command.server", "bungeecord.command.list"});
            bungeeSettings.permissions.Add("admin",new List<string>{"bungeecord.command.alert", "bungeecord.command.end", "bungeecord.command.ip", "bungeecord.command.reload"});

            WriteSettings(bungeeSettings);
        }
    }
}