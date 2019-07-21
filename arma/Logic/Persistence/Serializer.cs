using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.Logic.Persistence
{
    public sealed class Serializer
    {
        private static Serializer instance = null;

        private Serializer() { }

        public static Serializer Instance
        {
            get
            {
                if (instance == null)
                    instance = new Serializer();
                return instance;
            }
        }

        public void StoreServers(ObservableCollection<ServerViewModel> serverViewModels)
        {
            List<Server> servers = new List<Server>();
            foreach (ServerViewModel viewModel in serverViewModels)
            {
                if (viewModel.ServerTitle.Equals("Home"))
                {
                    continue;
                }
                servers.Add(viewModel.Server);
            }
            SerializeObject(servers,"persistence/servers.xml");
        }

        public ObservableCollection<ServerViewModel> LoadServers()
        {
            ObservableCollection<ServerViewModel> serverViewModels = new ObservableCollection<ServerViewModel>();
            
            
            List<Server> servers = DeSerializeObject <List<Server>>("persistence/servers.xml");
            if (servers == null)
            {
                return serverViewModels;
            }

            foreach (Server server in servers)
            {
                serverViewModels.Add(new ServerViewModel(server));
            }

            return serverViewModels;
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
            string folder = fileName.Substring(0, fileName.LastIndexOf("/"));
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
    }
}
