using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model.APIModels;
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
                return new WebClient().DownloadString(apiBaseURL + "ip").Trim();
            }
            //Fallback in case of API outage
            else
            {
                ErrorLogger.Append(new WebException("api.Fork.gg is not online or operational"));
                Console.WriteLine("api.Fork.gg is not online or operational");
                return new WebClient().DownloadString("http://icanhazip.com").Trim();
            }
        }

        public ForkVersion GetLatestForkVersion()
        {
            if (!IsAPIAvailable())
            {
                ErrorLogger.Append(new WebException("api.Fork.gg is not online or operational"));
                return ApplicationManager.Instance.CurrentForkVersion;
            }

            var response = RequestRawResponse(apiBaseURL + "versions/Fork/latest");
            string versionJson = RetrieveResponseBody(response);
            return JsonConvert.DeserializeObject<ForkVersion>(versionJson);
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

        private HttpWebResponse RequestRawResponse(string requestUrl)
        {
            WebRequest request = WebRequest.Create(requestUrl);
            request.Headers.Add("user-agent",ApplicationManager.UserAgent);
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