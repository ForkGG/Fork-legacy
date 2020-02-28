using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nihilus.Logic.Model;
using nihilus.Logic.Persistence.PersistencePO;

namespace nihilus.Logic.Persistence
{
    public class FileReader
    {
        public Dictionary<string, string> ReadServerSettings(string folderPath)
        {
            Dictionary<string, string> serverSettings = new Dictionary<string, string>();
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(folderPath+"/server.properties"))
                {
                    string line;
                    while ((line = sr.ReadLine())!=null)
                    {
                        if (!line.StartsWith("#"))
                        {
                            string[] args = line.Split('=');
                            serverSettings.Add(args[0],args[1]);
                        }
                    }
                }
                
                return serverSettings;
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static List<string> ReadWhiteListTxT(string path)
        {
            List<string> list = new List<string>();
            if (!File.Exists(path))
            {
                return new List<string>();
            }
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine())!=null)
                {
                    if (line.StartsWith("#")||line.Length==0)
                    {
                        continue;
                    }
                    if (line.EndsWith(","))
                    {
                        line = line.Remove(line.Length - 1);
                    }
                    list.Add(line);
                }
            }
            return list;
        }

        public static List<string> ReadWhiteListJson(string path)
        {
            if (!File.Exists(path))
            {
                return new List<string>();
            }
            string json;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    json = sr.ReadToEnd();
                }
            }
            List<WhitelistedPlayer> playerList = JsonConvert.DeserializeObject<List<WhitelistedPlayer>>(json);
            List<string> names = new List<string>();
            foreach (WhitelistedPlayer player in playerList)
            {
                names.Add(player.name);
            }
            return names;
        }

        public static List<string> ReadOPListTxT(string path)
        {
            //Using WhiteList method to avoid code duplication
            return ReadWhiteListTxT(path);
        }

        public static List<string> ReadOPListJson(string path)
        {
            if (!File.Exists(path))
            {
                return new List<string>();
            }
            string json;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    json = sr.ReadToEnd();
                }
            }
            List<OPPlayer> playerList = JsonConvert.DeserializeObject<List<OPPlayer>>(json);
            List<string> names = new List<string>();
            foreach (OPPlayer player in playerList)
            {
                names.Add(player.name);
            }
            return names;
        }

        public static List<string> ReadBanListTxT(string path)
        {
            if (!File.Exists(path))
            {
                return new List<string>();
            }
            List<string> list = new List<string>();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine())!=null)
                {
                    if (line.StartsWith("#")||line.Length==0)
                    {
                        continue;
                    }
                    string[] splittedLine = line.Split('|');
                    if (splittedLine.Length!=5)
                    {
                        continue;
                    }
                    list.Add(splittedLine[0]);
                }
            }
            return list;
        }

        public static List<string> ReadBanListJson(string path)
        {
            if (!File.Exists(path))
            {
                return new List<string>();
            }
            string json;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    json = sr.ReadToEnd();
                }
            }
            List<BannedPlayer> playerList = JsonConvert.DeserializeObject<List<BannedPlayer>>(json);
            List<string> names = new List<string>();
            foreach (BannedPlayer player in playerList)
            {
                names.Add(player.name);
            }
            return names;
        }
    }
}