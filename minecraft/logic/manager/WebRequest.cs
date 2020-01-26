using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using nihilus.Logic.Model;

namespace nihilus.Logic.Manager
{
    public class WebRequestManager
    {

        public List<ServerVersion> GetVanillaVersions()
        {
            string html = string.Empty;
            string url = "https://mcversions.net/";

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            
            Regex regex = new Regex("\"([0-9]+\\.[0-9]+\\.*[0-9]*)\"[\\s\\S]{0,300}href=\"(.*server.jar)\"");
            MatchCollection matches = regex.Matches(html);

            List<ServerVersion> vanillaVersions = new List<ServerVersion>();
            foreach (Match match in matches)
            {
                ServerVersion version = new ServerVersion();
                version.Version = match.Groups[1].Value;
                version.JarLink = match.Groups[2].Value;
                version.Type = ServerVersion.VersionType.Vanilla;
                
                vanillaVersions.Add(version);
            }
            vanillaVersions.Sort();
            vanillaVersions.Reverse();

            return vanillaVersions;
        }

        public List<ServerVersion> GetSpigotVersions()
        {
            ServerVersion version = new ServerVersion();
            version.Version = "test";
            version.JarLink = "NOT IMPLEMENTED";
            version.Type = ServerVersion.VersionType.Spigot;
            
            List<ServerVersion> serverVersions = new List<ServerVersion>();
            serverVersions.Add(version);

            return serverVersions;
        }
    }
}