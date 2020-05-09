using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using fork.Logic.Model;

namespace fork.Logic.Persistence
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
            File.WriteAllLines(Path.Combine(folderPath, "eula.txt"),lines,Encoding.UTF8);
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
                lines.Add(setting+"="+serverSettings[setting].Replace("\n","\\n").Replace("\r",""));
            }

            File.WriteAllLines(Path.Combine(folderPath,"server.properties"), lines, Encoding.UTF8);
        }
        
        public void AppendToErrorLog(string line)
        {
            FileInfo errorFile = new FileInfo(Path.Combine(App.ApplicationPath, "logs","errorLog.txt"));
            if (!errorFile.Exists)
            {
                if (!new DirectoryInfo(Path.Combine(App.ApplicationPath,"logs")).Exists)
                {
                    Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "logs"));
                }
                errorFile.Create();
            }

            try
            {
                using (StreamWriter sw = errorFile.AppendText())
                {
                    sw.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                if (typeof(Exception) != typeof(IOException))
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}