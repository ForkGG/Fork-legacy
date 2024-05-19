using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters;

public class FabricWebRequester
{
    public async Task<List<ServerVersion>> RequestFabricVersions()
    {
        string url = "https://meta.fabricmc.net/v2/versions/game";
        string json = await GetJsonFromUrl(url);

        FabricVersion[] fabricVersions = JsonConvert.DeserializeObject<FabricVersion[]>(json);
        string installerVersion = await GetLatestInstallerVersion();
        string loaderVersion = await GetMatchingLoaderVersion();

        List<ServerVersion> serverVersions = new();
        foreach (FabricVersion fabricVersion in fabricVersions)
        {
            if (!fabricVersion.stable)
            {
                continue;
            }

            serverVersions.Add(new ServerVersion
            {
                Type = ServerVersion.VersionType.Fabric,
                Version = fabricVersion.version,
                JarLink =
                    $"https://meta.fabricmc.net/v2/versions/loader/{fabricVersion.version}/{loaderVersion}/{installerVersion}/server/jar"
            });
        }

        return serverVersions;
    }

    private async Task<string> GetLatestInstallerVersion()
    {
        string url = "https://meta.fabricmc.net/v2/versions/installer/";
        string json = await GetJsonFromUrl(url);

        FabricInstallerVersion[] installerVersions = JsonConvert.DeserializeObject<FabricInstallerVersion[]>(json);
        return installerVersions.First(i => i.stable).version;
    }

    private async Task<string> GetMatchingLoaderVersion()
    {
        // TODO Currently all versions are compatible with the latest loader
        string url = "https://meta.fabricmc.net/v2/versions/loader";
        string json = await GetJsonFromUrl(url);

        FabricLoaderVersion[] loaderVersions = JsonConvert.DeserializeObject<FabricLoaderVersion[]>(json);
        return loaderVersions.First(l => l.stable).version;
    }

    private async Task<string> GetJsonFromUrl(string url)
    {
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
                    "Could not receive Fabric Versions (either meta.fabricmc.net is down or your Internet connection is not working)");
                return null;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "Error parsing response from meta.fabricmc.net! If you see this report it to a Fork maintainer.");
                return null;
            }
        }

        return json;
    }

    private record FabricVersion
    {
        public string version { get; set; }
        public bool stable { get; set; }
    }

    private record FabricInstallerVersion
    {
        public string url { get; set; }
        public string maven { get; set; }
        public string version { get; set; }
        public bool stable { get; set; }
    }

    private record FabricLoaderVersion
    {
        public string version { get; set; }
        public bool stable { get; set; }
    }
}