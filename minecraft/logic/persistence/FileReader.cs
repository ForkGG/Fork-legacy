using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using nihilus.Logic.Model;

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

        public List<Player> ReadWhiteList(string folderPath)
        {
            List<Player> players = new List<Player>();

            if (!File.Exists(folderPath+"/white-list.txt"))
            {
                return players;
            }
            
            using (var fs = new FileStream(folderPath+"/white-list.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    players.Add(new Player(line));
                }
            }
            
            
            return players;
        }
    }
}