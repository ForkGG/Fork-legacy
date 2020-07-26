using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using fork.Logic.Logging;
using Newtonsoft.Json;

namespace fork.Logic.WebRequesters
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
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
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


        private class PaperVersions
        {
            public string project;
            public List<string> versions;
        }
    }
}