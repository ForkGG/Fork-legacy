using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters;

public class PurpurWebRequester
{
    public async Task<List<ServerVersion>> RequestPurpurVersions()
    {
        string url = "https://api.purpurmc.org/v2/purpur";
        string json = ResponseCache.Instance.UncacheResponse(url);
        if (json == null)
        {
            try
            {
                Uri uri = new(url);
                HttpWebRequest request = WebRequest.CreateHttp(uri);
                request.UserAgent = ApplicationManager.UserAgent;
                using (WebResponse response = request.GetResponse())
                await using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new(stream))
                    json = await reader.ReadToEndAsync();

                ResponseCache.Instance.CacheResponse(url, json);
            }
            catch (WebException e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "Could not receive Purpur Versions (either api.purpurmc.org is down or your Internet connection is not working)");
                return new List<ServerVersion>();
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "Error parsing response from api.purpurmc.org! If you see this report it to a Fork maintainer.");
                return new List<ServerVersion>();
            }
        }

        PurpurVersions purpurVersions = JsonConvert.DeserializeObject<PurpurVersions>(json);
        List<ServerVersion> result = new();
        
        foreach (dynamic version in purpurVersions.versions)
        {
            ServerVersion serverVersion = new();
            serverVersion.Type = ServerVersion.VersionType.Purpur;
            serverVersion.Version = version;
            serverVersion.JarLink = $"https://api.purpurmc.org/v2/purpur/{version}/latest/download";
            result.Add(serverVersion);
        }

        result.Reverse();
        return result;
    }

    private record PurpurVersions
    {
        public string project { get; set; }
        public string[] versions { get; set; }
    }
}