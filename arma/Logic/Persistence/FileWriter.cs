using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace nihilus.Logic.Persistence
{
    public class FileWriter
    {
        public void WriteEula(string folderPath)
        {
            if (!new DirectoryInfo(folderPath).Exists)
            {
                Directory.CreateDirectory(folderPath);
            }
            DateTime dt = DateTime.Now;
            
            string date = String.Format("#{0:ddd MMM dd HH:mm:ss yyyy}",dt);
            string[] lines =
            {
                "#By changing the setting below to TRUE you are indicating your agreement to our EULA (https://account.mojang.com/documents/minecraft_eula).",
                date,
                "eula=true"
            };
            File.WriteAllLines(folderPath+"/eula.txt",lines,Encoding.UTF8);
        }

        public void WriteServerSettings(string folderPath, Dictionary<string, string> serverSettings)
        {
            if (!new DirectoryInfo(folderPath).Exists)
            {
                Directory.CreateDirectory(folderPath);
            }
            
            List<string> lines = new List<string>();
            lines.Add("#Minecraft server properties");
            DateTime dt = DateTime.Now;
            lines.Add(String.Format("#{0:ddd MMM dd HH:mm:ss yyyy}",dt));
            foreach (var setting in serverSettings.Keys)
            {
                lines.Add(setting+"="+serverSettings[setting]);
            }
            File.WriteAllLines(folderPath+ "/server.properties", lines, Encoding.UTF8);
        }
    }
}