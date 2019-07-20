using System;
using System.Collections.Generic;
using System.IO;

namespace nihilus.logic.persistence
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
                System.Console.WriteLine("The file could not be read:");
                System.Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}