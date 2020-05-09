using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using fork.Logic.Model;
using fork.Logic.Model.MinecraftVersionPojo;
using fork.Logic.WebRequesters;
using fork.ViewModel;

namespace fork.Logic.Manager
{
    public sealed class WebRequestManager
    {
        private static WebRequestManager instance;
        public static WebRequestManager Instance
        {
            get 
            {
                if (instance == null)
                    instance = new WebRequestManager();
                return instance;
            }
        }
        private WebRequestManager(){}
        
        private Dictionary<Manifest.VersionType,List<ServerVersion>> vanillaDict = new Dictionary<Manifest.VersionType, List<ServerVersion>>();
        private Dictionary<Manifest.VersionType, DateTime> vanillaCacheAge = new Dictionary<Manifest.VersionType, DateTime>();

        public List<ServerVersion> GetVanillaVersions(Manifest.VersionType type)
        {
            List<ServerVersion> cachedVersions = GetVanillaVersionsFromCache(type);
            if (cachedVersions!=null)
            {
                return cachedVersions;
            }
            
            Uri uri = new Uri("https://launchermeta.mojang.com/mc/game/version_manifest.json");
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            string json;
            using (var response =  request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }
            
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(json);
            List<ServerVersion> result = new List<ServerVersion>();
            foreach (Manifest.Version version in manifest.versions)
            {
                if (version.type == type)
                {
                    ServerVersion internalVersion = new ServerVersion();
                    internalVersion.Type = ServerVersion.VersionType.Vanilla;
                    internalVersion.Version = version.id;
                    internalVersion.JarLink = GetJarURL(version.url);
                    if (internalVersion.JarLink!=null)
                    {
                        result.Add(internalVersion);
                    }
                }
            }
            vanillaDict[type] = result;
            vanillaCacheAge[type] = DateTime.Now;
            return result;
        }

        public List<ServerVersion> GetPaperVersions()
        {
            PaperWebRequester paperWebRequester = new PaperWebRequester();
            List<string> versionStrings = paperWebRequester.RequestPaperVersions();

            List<ServerVersion> versions = new List<ServerVersion>();
            if (versionStrings == null)
            {
                Console.Error.WriteLine("[CRITICAL] No Paper versions found!");
                return versions;
            }
            foreach (string version in versionStrings)
            {
                ServerVersion serverVersion = new ServerVersion();
                serverVersion.Type = ServerVersion.VersionType.Paper;
                serverVersion.Version = version;
                serverVersion.JarLink = "https://papermc.io/api/v1/paper/" + version + "/latest/download";
                versions.Add(serverVersion);
            }

            return versions;
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

        private string GetJarURL(string jsonURL)
        {
            Uri uri = new Uri(jsonURL);
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            string json;
            using (var response =  request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            VersionDetails versionDetails = JsonConvert.DeserializeObject<VersionDetails>(json);

            if (versionDetails.downloads.server==null)
            {
                return null;
            }
            return versionDetails.downloads.server.url;
        }

        private List<ServerVersion> GetVanillaVersionsFromCache(Manifest.VersionType versionType)
        {
            //TODO File cache
            if (vanillaDict.ContainsKey(versionType)&&VanillaCacheUpToDate(versionType))
            {
                return vanillaDict[versionType];
            }

            return null;
        }

        private bool VanillaCacheUpToDate(Manifest.VersionType versionType)
        {
            if (vanillaCacheAge.ContainsKey(versionType))
            {
                DateTime age = vanillaCacheAge[versionType];
                DateTime now = DateTime.Now;
                TimeSpan difference = now - age;
                if (difference.TotalHours<12)
                {
                    return true;
                }
            }

            return false;
        }
    }
}