using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters;

public class PaperWebRequester
{
    public List<string> RequestPaperVersions()
    {
        string url = "https://api.papermc.io/v2/projects/paper";
        string json = ResponseCache.Instance.UncacheResponse(url);
        if (json == null)
        {
            try
            {
                Uri uri = new(url);
                HttpWebRequest request = WebRequest.CreateHttp(uri);
                request.UserAgent = ApplicationManager.UserAgent;
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new(stream))
                    json = reader.ReadToEnd();

                ResponseCache.Instance.CacheResponse(url, json);
            }
            catch (WebException e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "Could not receive Paper Versions (either papermc.io is down or your Internet connection is not working)");
                return new List<string>();
            }
        }

        PaperVersions paperVersions = JsonConvert.DeserializeObject<PaperVersions>(json);

        if (paperVersions == null || !paperVersions.project_id.Equals("paper"))
        {
            return null;
        }

        return paperVersions.versions.Reverse().ToList();
    }

    public async Task<int> RequestLatestBuildId(string version)
    {
        string url = "https://api.papermc.io/v2/projects/paper/versions/" + version;
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.UserAgent = ApplicationManager.UserAgent;
                using WebResponse response = request.GetResponse();
                await using Stream stream = response.GetResponseStream();
                using StreamReader reader = new(stream);
                string json = await reader.ReadToEndAsync();
                PaperVersion obj = JsonConvert.DeserializeObject<PaperVersion>(json);
                ;
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


    private class PaperVersions
    {
        public string project_id;
        public string project_name;
        public string[] version_groups;
        public string[] versions;
    }

    private class PaperVersion
    {
        public int[] builds;
        public string project_id;
        public string project_name;
        public string version;
    }
}