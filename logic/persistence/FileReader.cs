using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fork.Logic.Model;
using Fork.Logic.Persistence.PersistencePO;
using Newtonsoft.Json;

namespace Fork.Logic.Persistence;

public class FileReader
{
    public Dictionary<string, string> ReadServerSettings(string folderPath)
    {
        string propertiesPath = Path.Combine(folderPath, "server.properties");
        FileInfo propertiesFile = new(propertiesPath);
        if (!propertiesFile.Exists)
        {
            Console.WriteLine("Could not find properties file: " + propertiesPath +
                              "\nCreating default server.properties file");
            new FileWriter().WriteServerSettings(folderPath, new ServerSettings("world").SettingsDictionary);
        }

        Dictionary<string, string> serverSettings = new();
        try
        {
            // Open the text file using a stream reader.
            using (StreamReader sr = new(propertiesFile.FullName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    if (!line.StartsWith("#"))
                    {
                        string[] args = line.Split('=');
                        serverSettings.Add(args[0], args[1].Replace("\\n", "\n"));
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
        List<string> list = new();
        if (!File.Exists(path))
        {
            return new List<string>();
        }

        FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using (StreamReader sr = new(fs))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.Length == 0)
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
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader sr = new(fs))
            json = sr.ReadToEnd();
        List<WhitelistedPlayer> playerList = JsonConvert.DeserializeObject<List<WhitelistedPlayer>>(json);
        List<string> names = new();
        if (playerList != null)
        {
            names.AddRange(playerList.Select(player => player.name));
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
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader sr = new(fs))
            json = sr.ReadToEnd();
        List<OPPlayer> playerList = JsonConvert.DeserializeObject<List<OPPlayer>>(json);
        List<string> names = new();
        foreach (OPPlayer player in playerList) names.Add(player.name);
        return names;
    }

    public static List<string> ReadBanListTxT(string path)
    {
        if (!File.Exists(path))
        {
            return new List<string>();
        }

        List<string> list = new();
        FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using (StreamReader sr = new(fs))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.Length == 0)
                {
                    continue;
                }

                string[] splittedLine = line.Split('|');
                if (splittedLine.Length != 5)
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
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader sr = new(fs))
            json = sr.ReadToEnd();
        List<BannedPlayer> playerList = JsonConvert.DeserializeObject<List<BannedPlayer>>(json);
        List<string> names = new();
        foreach (BannedPlayer player in playerList) names.Add(player.name);
        return names;
    }

    public static bool IsFileReadable(FileInfo file)
    {
        // If the file can be opened for exclusive access it means that the file
        // is no longer locked by another process.
        try
        {
            using (FileStream inputStream = file.OpenRead())
                return inputStream.Length >= 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}