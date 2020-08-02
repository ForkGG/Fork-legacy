using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Fork.Logic.Logging;
using Newtonsoft.Json;
using Fork.Logic.Model;
using Fork.Logic.Model.MinecraftVersionModels;
using Fork.Logic.WebRequesters;
using Fork.ViewModel;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Fork.Logic.Manager
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
            return GetVanillaVersionsFromCache(type);
            
            /*List<ServerVersion> cachedVersions = GetVanillaVersionsFromCache(type);
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
            return result;*/
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
        
        private void CacheVanillaVersions(Manifest.VersionType versionType, List<ServerVersion> versions){
            DateTime cacheAge = DateTime.Now;
            
            //RAM cache
            vanillaDict[versionType] = versions;
            vanillaCacheAge[versionType] = cacheAge;
            VanillaVersionCache versionCache = new VanillaVersionCache(cacheAge, versionType, versions);

            //File cache
            string json = System.Text.Json.JsonSerializer.Serialize(versionCache);
            try
            {
                string path = Path.Combine(App.ApplicationPath,"persistence");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, "vanilla-"+versionType+".json");
                File.WriteAllText(path,json, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("Error while saving Vanilla version cache. Skipping...");
            }
        }

        private List<ServerVersion> GetVanillaVersionsFromCache(Manifest.VersionType versionType)
        {
            if (vanillaDict.ContainsKey(versionType)&&VanillaCacheUpToDate(versionType))
            {
                return vanillaDict[versionType];
            }
            string path = Path.Combine(App.ApplicationPath, "persistence", "vanilla-" + versionType + ".json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path,Encoding.UTF8);
                    VanillaVersionCache versionCache = JsonSerializer.Deserialize<VanillaVersionCache>(json);
                    if (DateTime.Now.Subtract(versionCache.CacheCreation).Hours < 12)
                    {
                        return versionCache.Versions;
                    }
                    UpdateVanillaVersionsAsync(versionType);
                    return versionCache.Versions;
                }
                catch (Exception e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Error while reading Vanilla version cache. Skipping...");
                }
            }
            UpdateVanillaVersions(versionType);
            return GetVanillaVersionsFromCache(versionType);
        }

        private void UpdateVanillaVersions(Manifest.VersionType versionType)
        {
            Uri uri = new Uri("https://launchermeta.mojang.com/mc/game/version_manifest.json");
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            string json;
            try
            {
                using (var response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }
            } catch(WebException e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("WebException while updating Vanilla Versions (Either mojang.com is down or your internet coeenction is not working)");
                return;
            }
            
            
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(json);
            List<ServerVersion> result = new List<ServerVersion>();
            foreach (Manifest.Version version in manifest.versions)
            {
                if (version.type == versionType)
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
            CacheVanillaVersions(versionType, result);
        }

        private void UpdateVanillaVersionsAsync(Manifest.VersionType versionType)
        {
            new Thread(() =>
            {
                UpdateVanillaVersions(versionType);
            }).Start();
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
        
        private class VanillaVersionCache
        {
            public DateTime CacheCreation { get; set; }
            public Manifest.VersionType VersionType { get; set; }
            public List<ServerVersion> Versions { get; set; }

            public VanillaVersionCache(DateTime cacheCreation, Manifest.VersionType versionType,
                List<ServerVersion> versions)
            {
                CacheCreation = cacheCreation;
                VersionType = versionType;
                Versions = versions;
            }

            //Empty constructor for serializers
            public VanillaVersionCache()
            { }
        }
    }
}