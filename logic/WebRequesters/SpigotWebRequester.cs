using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters;

public class SpigotWebRequester
{
    public async Task<List<ServerVersion>> RequestSpigotVersions()
    {
        //string url = "https://serverjars.com/api/fetchAll/spigot";
        string url = "https://hub.spigotmc.org/versions/";
        string json = ResponseCache.Instance.UncacheResponse(url);
        if (json == null)
        {
            try
            {
                string raw = null;

                Uri uri = new(url);
                HttpWebRequest request = WebRequest.CreateHttp(uri);
                request.UserAgent = ApplicationManager.UserAgent;
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new(stream))
                    raw = await reader.ReadToEndAsync();

                HtmlDocument doc = new();
                doc.LoadHtml(raw);

                HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links == null)
                {
                    throw new Exception("Invalid response from hub.spigotmc.org. No version data found");
                }

                Collection<string> groupVersions = new();
                Regex spRegex = new("^[0-9]+\\.[0-9]+(\\.[0-9]+)?$");

                foreach (HtmlNode link in links)
                {
                    string href = link.Attributes["href"].Value;
                    if (!href.EndsWith(".json"))
                    {
                        continue; //Ignore non-json if any
                    }

                    string name = href[..^5];

                    if (!name.Contains('.'))
                    {
                        continue;
                    }

                    if (spRegex.IsMatch(name))
                    {
                        groupVersions.Add(name);
                    }
                }

                var vResponse = new
                {
                    response = groupVersions.OrderByDescending(v => new Version(v)).ToList()
                };

                json = JsonConvert.SerializeObject(vResponse, Formatting.Indented);
                ResponseCache.Instance.CacheResponse(url, json);
            }
            catch (WebException e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "Could not receive Spigot Versions (either serverjars.com is down or your Internet connection is not working)");
                return new List<ServerVersion>();
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "Error parsing response from serverjars.com! If you see this report it to a Fork maintainer.");
                return new List<ServerVersion>();
            }
        }

        dynamic dyn = JsonConvert.DeserializeObject(json);
        dyn = dyn.response;
        List<ServerVersion> result = new();
        Version snapshotMaxVersion = new("1.10.2");
        Version getbukkitMinVersion = new("1.16.5");
        /*
         * So apparently, this is the latest version in where spigot
         * used -snapshot thing when downloading. Regardless of the
         * -snapshot prefix, you are alaways downloading the latest
         * build
         */

        string downloadURL = "https://download.getbukkit.org/spigot/spigot-{version}.jar";
        foreach (string version in dyn)
        {
            Version current = new(version);
            int legacyComp = current.CompareTo(snapshotMaxVersion);
            int bukkitComp = current.CompareTo(getbukkitMinVersion);
            if (legacyComp <= 0)
            {
                //Legacy with prefix and CDN
                downloadURL = "https://cdn.getbukkit.org/spigot/spigot-{version}-R0.1-SNAPSHOT-latest.jar";
            }

            if (bukkitComp <= 0 && legacyComp == 1)
            {
                downloadURL = "https://cdn.getbukkit.org/spigot/spigot-{version}.jar";
            }

            ServerVersion serverVersion = new();
            serverVersion.Type = ServerVersion.VersionType.Spigot;
            serverVersion.Version = version;
            serverVersion.JarLink = downloadURL.Replace("{version}", version);
            result.Add(serverVersion);
        }

        return result;
    }
}