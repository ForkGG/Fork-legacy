using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Fork.Logic.Persistence;

public class FileWriter
{
    public void WriteEula(string folderPath)
    {
        if (!new DirectoryInfo(folderPath).Exists)
        {
            Directory.CreateDirectory(folderPath);
        }

        DateTime dt = DateTime.Now;

        string date = string.Format("#{0:ddd MMM dd HH:mm:ss yyyy}", dt);
        string[] lines =
        {
            "#By changing the setting below to TRUE you are indicating your agreement to our EULA (https://account.mojang.com/documents/minecraft_eula).",
            date,
            "eula=true"
        };
        File.WriteAllLines(Path.Combine(folderPath, "eula.txt"), lines, Encoding.UTF8);
    }

    public async Task WriteServerSettings(string folderPath, IDictionary<string, string> serverSettings)
    {
        if (!new DirectoryInfo(folderPath).Exists)
        {
            Directory.CreateDirectory(folderPath);
        }

        List<string> lines = new();
        lines.Add("#Minecraft server properties");
        DateTime dt = DateTime.Now;
        lines.Add(string.Format("#{0:ddd MMM dd HH:mm:ss yyyy}", dt));
        foreach (string setting in serverSettings.Keys)
            lines.Add(setting + "=" + serverSettings[setting].Replace("\n", "\\n").Replace("\r", ""));

        await File.WriteAllLinesAsync(Path.Combine(folderPath, "server.properties"), lines, Encoding.UTF8);
    }

    public void AppendToErrorLog(string line)
    {
        FileInfo errorFile = new(Path.Combine(App.ApplicationPath, "logs", "errorLog.txt"));
        if (!errorFile.Exists)
        {
            errorFile.Create().Close();
        }

        try
        {
            using (StreamWriter sw = errorFile.AppendText()) sw.WriteLine(line);
        }
        catch (Exception e)
        {
            if (typeof(Exception) != typeof(IOException))
            {
                Console.WriteLine("Exception occured during Exception writing:\n" + e.Message + "\n" + e.StackTrace +
                                  "\n\nError was: " + line);
            }
        }
    }

    public static bool IsFileWritable(FileInfo file)
    {
        // If the file can be opened for exclusive access it means that the file
        // is no longer locked by another process.
        try
        {
            using (FileStream outputStream = file.OpenWrite())
                return outputStream.CanWrite;
        }
        catch (Exception)
        {
            return false;
        }
    }
}