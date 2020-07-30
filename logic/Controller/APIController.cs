using System;
using System.IO;
using System.Net;
using fork.Logic.Logging;
using fork.Logic.Manager;
using fork.Logic.Model.APIModel;
using Newtonsoft.Json;

namespace fork.Logic.Controller
{
    public class APIController
    {
        private string apiBaseURL = "https://api.fork.gg/";

        public string GetExternalIPAddress()
        {
            if (IsAPIAvailable())
            {
                return new WebClient().DownloadString(apiBaseURL + "ip").Trim();
            }
            //Fallback in case of API outage
            else
            {
                ErrorLogger.Append(new WebException("api.fork.gg is not online or operational"));
                Console.WriteLine("api.fork.gg is not online or operational");
                return new WebClient().DownloadString("http://icanhazip.com").Trim();
            }
        }

        public ForkVersion GetLatestForkVersion()
        {
            if (!IsAPIAvailable())
            {
                ErrorLogger.Append(new WebException("api.fork.gg is not online or operational"));
                return ApplicationManager.Instance.CurrentForkVersion;
            }

            try
            {
                var response = RequestRawResponse(apiBaseURL + "versions/fork/latest");
                string versionJson = RetrieveResponseBody(response);
                return JsonConvert.DeserializeObject<ForkVersion>(versionJson);
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return ApplicationManager.Instance.CurrentForkVersion;
            }
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