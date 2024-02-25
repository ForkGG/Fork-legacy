using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Fork.Logic.Model.Settings;
using YamlDotNet.Serialization;
using Server = Fork.Logic.Model.Settings.Server;

namespace Fork.Logic.Persistence.YMLReaders;

public class BungeeSettingsSerializer
{
    private readonly object fileLock = new();

    public BungeeSettingsSerializer(FileInfo configYml)
    {
        ConfigYML = configYml;
    }

    public FileInfo ConfigYML { get; }

    public BungeeSettings ReadSettings()
    {
        if (!ConfigYML.Exists)
        {
            InitializeBungeeConfig();
        }

        lock (fileLock)
        {
            using (StreamReader streamReader = new(ConfigYML.OpenRead()))
            {
                IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                return deserializer.Deserialize<BungeeSettings>(streamReader);
            }
        }
    }

    public void WriteSettings(BungeeSettings settings)
    {
        int i = 0;
        while (!FileWriter.IsFileWritable(ConfigYML))
        {
            if (i > 60 * 5)
            {
                Console.WriteLine("File " + ConfigYML.FullName + " was not writable for 5 minutes, aborting...");
                return;
            }

            Thread.Sleep(1000);
            i++;
        }

        lock (fileLock)
        {
            //TODO this can cause crash if file is somehow locked
            using (StreamWriter streamWriter = new(ConfigYML.Create()))
            {
                Serializer serializer = new();
                serializer.Serialize(streamWriter, settings);
            }
        }
    }

    private void InitializeBungeeConfig()
    {
        BungeeSettings bungeeSettings = new();
        bungeeSettings.groups.Add("md_5", new List<string> { "admin" });
        bungeeSettings.disabled_commands.Add("disabledcommandhere");
        bungeeSettings.servers.Add("lobby", new Server());
        bungeeSettings.listeners = new List<Listener> { new() };
        bungeeSettings.listeners[0].priorities.Add("lobby");
        bungeeSettings.permissions.Add("default",
            new List<string> { "bungeecord.command.server", "bungeecord.command.list" });
        bungeeSettings.permissions.Add("admin",
            new List<string>
            {
                "bungeecord.command.alert", "bungeecord.command.end", "bungeecord.command.ip",
                "bungeecord.command.reload"
            });

        WriteSettings(bungeeSettings);
    }
}