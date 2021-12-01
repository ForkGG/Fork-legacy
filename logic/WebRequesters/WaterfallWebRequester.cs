using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters
{
    public class WaterfallWebRequester
    {
        public ServerVersion RequestLatestWaterfallVersion()
        {
            string url = "https://papermc.io/api/v2/projects/waterfall/";
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
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
                    Console.WriteLine("WebException while requesting latest Waterfall Version (Either papermc.io is down or your internet connection is not working)");
                    return null;
                }                
            }

            WaterfallVersions waterfallVersions = JsonConvert.DeserializeObject<WaterfallVersions>(json);
            if (waterfallVersions == null || !waterfallVersions.project_id.Equals("waterfall"))
            {
                return null;
            }
            
            ServerVersion waterfallVersion = new ServerVersion();
            waterfallVersion.Type = ServerVersion.VersionType.Waterfall;
            waterfallVersion.Version = waterfallVersions.versions[0];
            waterfallVersion.JarLink = "https://papermc.io/api/v1/waterfall/" + waterfallVersions.versions[0] + "/latest/download";

            return waterfallVersion;
        }
        
        private class WaterfallVersions
        {
            public string project_id;
            public List<string> versions;
        }
    }
}