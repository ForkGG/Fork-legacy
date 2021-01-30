using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters
{
    public class PaperWebRequester
    {
        public List<string> RequestPaperVersions()
        {
            string url = "https://papermc.io/api/v1/paper/";
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = WebRequest.CreateHttp(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                } catch(WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Paper Versions (either papermc.io is down or your Internet connection is not working)");
                    return new List<string>();
                }                                
            }

            PaperVersions paperVersions = JsonConvert.DeserializeObject<PaperVersions>(json);

            if (paperVersions == null || !paperVersions.project.Equals("paper"))
            {
                return null;
            }
            return paperVersions.versions;
        }

        public async Task<int> RequestLatestBuildId(string version)
        {
            string url = "https://papermc.io/api/v1/paper/"+version+"/latest/";
            {
                try
                {
                    HttpWebRequest request = WebRequest.CreateHttp(url);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using var response = request.GetResponse();
                    await using Stream stream = response.GetResponseStream();
                    using StreamReader reader = new StreamReader(stream);
                    string json = await reader.ReadToEndAsync();
                    dynamic obj = JsonConvert.DeserializeObject(json);
                    return obj.build;
                }
                catch (Exception e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not get latest build id for paper version "+version);
                    return 0;
                }
            }
        }


        private class PaperVersions
        {
            public string project;
            public List<string> versions;
        }
    }
}