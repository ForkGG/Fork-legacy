using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using fork.Logic.Model;
using fork.Logic.Model.ProxyModels;
using fork.ViewModel;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace fork.Logic.Persistence
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
        
        public void StoreEntities(ObservableCollection<EntityViewModel> entityViewModels)
        {
            EntityLists entities = new EntityLists();
            foreach (EntityViewModel viewModel in entityViewModels)
            {
                switch (viewModel.Entity)
                {
                    case Server server:
                        entities.ServerList.Add(server);
                        break;
                    case Network network:
                        entities.NetworkList.Add(network);
                        break;
                }
            }

            string json = JsonConvert.SerializeObject(entities, Formatting.Indented);
            FileInfo entitiesFile = new FileInfo(Path.Combine(App.ApplicationPath, "persistence", "entities.json"));
            lock (writeLock)
            {
                using (FileStream fs = entitiesFile.Create())
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(json);
                }
            }
        }

        public ObservableCollection<EntityViewModel> LoadEntities()
        {
            ObservableCollection<EntityViewModel> entityViewModels = new ObservableCollection<EntityViewModel>();
            
            // Legacy stuff to support older Fork versions
            FileInfo serversFile = new FileInfo(Path.Combine(App.ApplicationPath,"persistence","servers.xml"));
            if (serversFile.Exists)
            {
                List<Server> servers = DeSerializeObject <List<Server>>(serversFile.FullName);
                if (servers!=null)
                {
                    foreach (Server server in servers)
                    {
                        entityViewModels.Add(new ServerViewModel(server));
                    }
                }
                serversFile.Delete();
            }
            
            FileInfo entitiesFile = new FileInfo(Path.Combine(App.ApplicationPath, "persistence", "entities.json"));
            if (!entitiesFile.Exists)
            {
                return entityViewModels;
            }
            EntityLists entities = JsonConvert.DeserializeObject<EntityLists>(File.ReadAllText(entitiesFile.FullName));
            if (entities != null)
            {
                foreach (Server server in entities.ServerList)
                {
                    ServerViewModel serverViewModel = new ServerViewModel(server);
                    if (!entityViewModels.Contains(serverViewModel))
                    {
                        entityViewModels.Add(serverViewModel);
                    }
                }

                foreach (Network network in entities.NetworkList)
                {
                    entityViewModels.Add(new NetworkViewModel(network));
                }
            }

            //Overwrite json if any changes appeared while deserializing (Unset value set etc.)
            StoreEntities(entityViewModels);
            return entityViewModels;
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        private void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            fileName.Trim();
            string folder = fileName.Substring(0, fileName.LastIndexOf("\\"));
            if (!new DirectoryInfo(folder).Exists)
            {
                Directory.CreateDirectory(folder);
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex.StackTrace);
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex.StackTrace);
            }

            return objectOut;
        }
        
        private class EntityLists
        {
            public List<Server> ServerList = new List<Server>();
            public List<Network> NetworkList = new List<Network>();
        }
    }
}
