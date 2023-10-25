using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters
{
    public class SpigotWebRequester
    {
        public async Task<List<ServerVersion>> RequestSpigotVersions()
        {
            string url = "https://serverjars.com/api/fetchAll/modded/fabric";
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
                        json = await reader.ReadToEndAsync();
                    }

                    dynamic fullResponse = JsonConvert.DeserializeObject(json);
                    if (!fullResponse.status.ToString().Equals("success"))
                    {
                        throw new Exception("Invalid response from serverjars.com. No or wrong status found!");
                    }
                    ResponseCache.Instance.CacheResponse(url, json);
                }
                catch (WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine(
                        "Could not receive Spigot Versions (either serverjars.com is down or your Internet connection is not working)");
                    return new ();
                }
                catch (Exception e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine(
                        "Error parsing response from serverjars.com! If you see this report it to a Fork maintainer.");
                    return new ();
                }                              
            }

            dynamic dyn = JsonConvert.DeserializeObject(json);
            dyn = dyn.response;
            List<ServerVersion> result = new ();
            foreach (dynamic version in dyn)
            {
                ServerVersion serverVersion = new ServerVersion();
                serverVersion.Type = ServerVersion.VersionType.Spigot;
                serverVersion.Version = version.version;
                serverVersion.JarLink = "https://serverjars.com/api/fetchJar/modded/fabric/" + version.version;
                result.Add(serverVersion);
            }

            return result;
        }
    }
}