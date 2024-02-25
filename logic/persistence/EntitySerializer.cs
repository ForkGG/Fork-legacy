using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.ProxyModels;
using Fork.ViewModel;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Fork.Logic.Persistence;

public sealed class EntitySerializer
{
    private static EntitySerializer instance;
    private static readonly object writeLock = new();

    private EntitySerializer()
    {
    }

    public static EntitySerializer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EntitySerializer();
            }

            return instance;
        }
    }

    public void StoreEntities()
    {
        EntityLists entities = new();
        foreach (EntityViewModel viewModel in ServerManager.Instance.Entities)
            switch (viewModel.Entity)
            {
                case Server server:
                    entities.ServerList.Add(server);
                    break;
                case Network network:
                    entities.NetworkList.Add(network);
                    break;
            }
#if DEBUG
        string json = JsonConvert.SerializeObject(entities, Formatting.Indented);
#else
            string json = JsonConvert.SerializeObject(entities, Formatting.None);
#endif
        DirectoryInfo directoryInfo = new(Path.Combine(App.ApplicationPath, "persistence"));
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        FileInfo entitiesFile = new(Path.Combine(App.ApplicationPath, "persistence", "entities.json"));
        lock (writeLock)
        {
            using (FileStream fs = entitiesFile.Create())
            using (StreamWriter sw = new(fs))
                sw.WriteLine(json);
        }
    }

    public ObservableCollection<Entity> LoadEntities()
    {
        FileInfo entitiesFile = new(Path.Combine(App.ApplicationPath, "persistence", "entities.json"));
        ObservableCollection<Entity> entityViewModels = new();
        if (!entitiesFile.Exists)
        {
            return entityViewModels;
        }

        EntityLists entities = JsonConvert.DeserializeObject<EntityLists>(File.ReadAllText(entitiesFile.FullName));
        if (entities != null)
        {
            foreach (Server server in entities.ServerList)
            {
                DirectoryInfo serverDir = new(Path.Combine(App.ServerPath, server.Name));
                if (!serverDir.Exists)
                {
                    ErrorLogger.Append(
                        new Exception("Could not find server " + server.Name +
                                      " that was listed in entities.json . Removing it..."));
                    continue;
                }

                if (!entityViewModels.Contains(server))
                {
                    entityViewModels.Add(server);
                }
            }

            foreach (Network network in entities.NetworkList)
            {
                DirectoryInfo serverDir = new(Path.Combine(App.ServerPath, network.Name));
                if (!serverDir.Exists)
                {
                    ErrorLogger.Append(
                        new Exception("Could not find network " + network.Name +
                                      " that was listed in entities.json . Removing it..."));
                    continue;
                }

                entityViewModels.Add(network);
            }
        }

        return entityViewModels;
    }

    private class EntityLists
    {
        public readonly List<Network> NetworkList = new();
        public readonly List<Server> ServerList = new();
    }
}