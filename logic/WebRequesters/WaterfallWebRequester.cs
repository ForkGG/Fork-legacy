using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters;

public class WaterfallWebRequester
{
    public async Task<ServerVersion> RequestLatestWaterfallVersion()
    {
        string url = "https://api.papermc.io/v2/projects/waterfall";
        string json = ResponseCache.Instance.UncacheResponse(url);
        if (json == null)
        {
            try
            {
                Uri uri = new(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = ApplicationManager.UserAgent;
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new(stream))
                    json = await reader.ReadToEndAsync();

                ResponseCache.Instance.CacheResponse(url, json);
            }
            catch (WebException e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "WebException while requesting latest Waterfall Version (Either papermc.io is down or your internet connection is not working)");
                return null;
            }
        }

        WaterfallVersions waterfallVersions = JsonConvert.DeserializeObject<WaterfallVersions>(json);
        if (waterfallVersions == null || !waterfallVersions.project_id.Equals("waterfall"))
        {
            return null;
        }

        string version = waterfallVersions.versions.Reverse().FirstOrDefault();
        int build = await RequestLatestBuildId(version);
        ServerVersion waterfallVersion = new();
        waterfallVersion.Type = ServerVersion.VersionType.Waterfall;
        waterfallVersion.Version = version;
        waterfallVersion.JarLink = "https://thatstupidpaperremovedv1api.madebyitoncek.repl.co/api/v1/waterfall/" +
                                   waterfallVersions.versions[0] + "/latest/download";
        waterfallVersion.JarLink =
            $"https://api.papermc.io/v2/projects/waterfall/versions/{version}/builds/{build}/downloads/waterfall-{version}-{build}.jar";


        return waterfallVersion;
    }

    private async Task<int> RequestLatestBuildId(string version)
    {
        string url = "https://api.papermc.io/v2/projects/waterfall/versions/" + version;
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.UserAgent = ApplicationManager.UserAgent;
                using WebResponse response = request.GetResponse();
                await using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string json = await reader.ReadToEndAsync();
                WaterfallVersion obj = JsonConvert.DeserializeObject<WaterfallVersion>(json);
                return obj.builds.LastOrDefault();
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("Could not get latest build id for paper version " + version);
                return 0;
            }
        }
    }

    private class WaterfallVersions
    {
        public string project_id;
        public string project_name;
        public string[] version_groups;
        public string[] versions;
    }

    private class WaterfallVersion
    {
        public int[] builds;
        public string project_id;
        public string project_name;
        public string version;
    }
}