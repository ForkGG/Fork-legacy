using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters;

public class PurpurWebRequester
{
    public async Task<List<ServerVersion>> RequestPurpurVersions()
    {
        string url = "https://serverjars.com/api/fetchAll/servers/purpur";
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
                    json = await reader.ReadToEndAsync();

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
                    "Could not receive Purpur Versions (either serverjars.com is down or your Internet connection is not working)");
                return new List<ServerVersion>();
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine(
                    "Error parsing response from serverjars.com! If you see this report it to a Fork maintainer.");
                return new List<ServerVersion>();
            }
        }

        dynamic dyn = JsonConvert.DeserializeObject(json);
        dyn = dyn.response;
        List<ServerVersion> result = new();
        foreach (dynamic version in dyn)
        {
            ServerVersion serverVersion = new();
            serverVersion.Type = ServerVersion.VersionType.Purpur;
            serverVersion.Version = version.version;
            serverVersion.JarLink = "https://serverjars.com/api/fetchJar/servers/purpur/" + version.version;
            result.Add(serverVersion);
        }

        return result;
    }
}