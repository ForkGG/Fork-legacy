using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms.VisualStyles;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model.APIModels;
using Fork.logic.model.PluginModels;
using Newtonsoft.Json;

namespace Fork.Logic.Controller
{
    public class APIController
    {
        public static Dictionary<string, CachedAPIResponse> ResponseCache = new();
        
        
        private string apiBaseURL = "https://api.Fork.gg/";

        /// <summary>
        /// Get public IP address of this current machine. This is cached for 24 hours by default
        /// </summary>
        /// <returns></returns>
        public string GetExternalIPAddress()
        {
            if (IsAPIAvailable())
            {
                return RetrieveResponseBody(RequestResponseWithCache(apiBaseURL + "ip", null, TimeSpan.FromHours(24))).Trim();
            }
            //Fallback in case of API outage
            ErrorLogger.Append(new WebException("api.Fork.gg is not online or operational"));
            Console.WriteLine("api.Fork.gg is not online or operational");
            return RetrieveResponseBody(RequestResponseWithCache("http://icanhazip.com", null, TimeSpan.FromHours(24))).Trim();
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

        private HttpWebResponse RequestResponseWithCache(string requestUrl, IDictionary<string, string> headers = null,
            TimeSpan? maxCacheAge = null)
        {
            if (maxCacheAge != null)
            {
                HttpWebResponse cachedResponse = ResponseFromCache(requestUrl, headers);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
            }

            HttpWebResponse response = RequestRawResponse(requestUrl, headers);
            if (maxCacheAge != null)
            {
                CacheResponse(HashRequest(requestUrl,headers), response, maxCacheAge.Value);
            }
            return response;
        }


        private void CacheResponse(string requestHash, HttpWebResponse response, TimeSpan maxCacheAge)
        {
            //This should never happen
            if (ResponseCache.ContainsKey(requestHash))
            {
                return;
            }
            ResponseCache.Add(requestHash, new CachedAPIResponse(requestHash, response, maxCacheAge));
        }
        
        private HttpWebResponse ResponseFromCache(string requestUrl, IDictionary<string, string> headers = null)
        {
            string requestHash = HashRequest(requestUrl, headers);
            if (ResponseCache.ContainsKey(requestHash))
            {
                return ResponseCache[requestHash].WebResponse;
            }
            return null;
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
        
        private string HashRequest(string requestUrl, IDictionary<string, string> headers = null)
        {
            string stringToHash;
            if (headers == null)
            {
                stringToHash = requestUrl;
            }
            else
            {
                stringToHash = requestUrl + HeaderDictToString(headers);
            }

            byte[] hashArray = MD5.HashData(Encoding.UTF8.GetBytes(stringToHash));

            StringBuilder result = new();
            foreach (byte b in hashArray)
            {
                result.Append(b.ToString("X2"));
            }
            return result.ToString();
        }

        private string HeaderDictToString(IDictionary<string, string> headers)
        {
            List<string> headerStrings = new();
            foreach (KeyValuePair<string,string> header in headers)
            {
                headerStrings.Add(header.Key+header.Value);
            }
            headerStrings.Sort();
            return string.Join("",headerStrings);
        }

        
    }
}