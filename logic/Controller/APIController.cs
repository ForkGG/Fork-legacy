using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model.APIModels;
using Fork.logic.model.PluginModels;
using Newtonsoft.Json;

namespace Fork.Logic.Controller
{
    public class APIController
    {
        private string apiBaseURL = "https://api.Fork.gg/";

        public string GetExternalIPAddress()
        {
            if (IsAPIAvailable())
            {
                return RetrieveResponseBody(RequestRawResponse(apiBaseURL + "ip")).Trim();
            }
            //Fallback in case of API outage
            else
            {
                ErrorLogger.Append(new WebException("api.Fork.gg is not online or operational"));
                Console.WriteLine("api.Fork.gg is not online or operational");
                return new WebClient().DownloadString("http://icanhazip.com").Trim();
            }
        }

        public ForkVersion GetLatestForkVersion(bool useBeta)
        {
            if (!IsAPIAvailable())
            {
                ErrorLogger.Append(new WebException("api.Fork.gg is not online or operational"));
                return ApplicationManager.Instance.CurrentForkVersion;
            }
            try
            {
                Dictionary<string, string> headers = null;
                if (useBeta)
                {
                    headers = new Dictionary<string, string>();
                    headers.Add("include-beta","true");
                }
                var response = RequestRawResponse(apiBaseURL + "versions/fork/latest", headers);
                string versionJson = RetrieveResponseBody(response);
                return JsonConvert.DeserializeObject<ForkVersion>(versionJson);
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return ApplicationManager.Instance.CurrentForkVersion;
            }
        }

        public List<string> GetPatrons()
        {
            if (!IsAPIAvailable())
            {
                ErrorLogger.Append(new WebException("api.Fork.gg is not online or operational"));
                return new List<string>();
            }
            try
            {
                var response = RequestRawResponse(apiBaseURL + "patrons");
                string patronsJson = RetrieveResponseBody(response);
                return JsonConvert.DeserializeObject<List<string>>(patronsJson);
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return new List<string>();
            }
        }

        public async Task<FileInfo> DownloadPluginAsync(InstalledPlugin plugin, string targetPath)
        {
            if (!IsAPIAvailable())
            {
                ErrorLogger.Append(new WebException("api.Fork.gg is not online or operational"));
                return null;
            }

            try
            {
                string pluginUrl = WebUtility.UrlEncode(plugin.Plugin.file.url);
                WebClient webClient = new WebClient();
                webClient.Headers.Add("user-agent",ApplicationManager.UserAgent);
                await webClient.DownloadFileTaskAsync(new Uri(apiBaseURL+"plugins/download?url="+pluginUrl), targetPath);
                
                FileInfo pluginFile = new FileInfo(targetPath);
                if (pluginFile.Exists)
                {
                    return pluginFile;
                }
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
            return null;
        }
        
        private bool IsAPIAvailable()
        {
            try
            {
                HttpWebResponse response = RequestRawResponse(apiBaseURL + "status");
                //Check if API is online
                return response.StatusCode == HttpStatusCode.OK && RetrieveResponseBody(response).Equals("ONLINE");
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private HttpWebResponse RequestRawResponse(string requestUrl, IDictionary<string, string> headers = null)
        {
            WebRequest request = WebRequest.Create(requestUrl);
            request.Headers.Add("user-agent",ApplicationManager.UserAgent);
            if (headers != null)
            {
                foreach (KeyValuePair<string,string> keyValuePair in headers)
                {
                    request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return (HttpWebResponse) request.GetResponse();
        }

        private string RetrieveResponseBody(WebResponse webResponse)
        {
            using (var responseStream = webResponse.GetResponseStream())
            {
                if (responseStream == null)
                {
                    return "";
                }
                using (var reader = new StreamReader(responseStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}