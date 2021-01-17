using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.Logic.Model.ProxyModels;
using Fork.ViewModel;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Fork.Logic.Persistence
{
    public sealed class EntitySerializer
    {
        private static EntitySerializer instance = null;
        private static object writeLock = new object();

        private EntitySerializer() { }

        public static EntitySerializer Instance
        {
            get
            {
                if (instance == null)
                    instance = new EntitySerializer();
                return instance;
            }
        }

        public ObservableCollection<Entity> LoadEntities()
        {
            FileInfo entitiesFile = new FileInfo(Path.Combine(App.ApplicationPath, "persistence", "entities.json"));
            if (!entitiesFile.Exists)
            {
                return null;
            }
            
            // Legacy stuff to support older Fork versions
            ObservableCollection<Entity> entityViewModels = new();
            EntityLists entities = JsonConvert.DeserializeObject<EntityLists>(File.ReadAllText(entitiesFile.FullName));
            if (entities != null)
            {
                foreach (Server server in entities.ServerList)
                {
                    DirectoryInfo serverDir = new DirectoryInfo(Path.Combine(App.ServerPath,server.Name));
                    if (!serverDir.Exists)
                    {
                        ErrorLogger.Append(
                            new Exception("Could not find server "+server.Name+" that was listed in entities.json . Removing it..."));
                        continue;
                    }
                    if (!entityViewModels.Contains(server))
                    {
                        entityViewModels.Add(server);
                    }
                }

                foreach (Network network in entities.NetworkList)
                {
                    DirectoryInfo serverDir = new DirectoryInfo(Path.Combine(App.ServerPath,network.Name));
                    if (!serverDir.Exists)
                    {
                        ErrorLogger.Append(
                            new Exception("Could not find network "+network.Name+" that was listed in entities.json . Removing it..."));
                        continue;
                    }
                    entityViewModels.Add(network);
                }
            }

            //entitiesFile.Delete();
            return entityViewModels;
        }

        private class EntityLists
        {
            public List<Server> ServerList = new List<Server>();
            public List<Network> NetworkList = new List<Network>();
        }
    }
}
