using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.Logic.Model.MinecraftVersionModels;
using Fork.Logic.WebRequesters;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Fork.Logic.Manager;

public sealed class WebRequestManager
{
    private static WebRequestManager instance;
    private readonly Dictionary<Manifest.VersionType, DateTime> vanillaCacheAge = new();

    private readonly Dictionary<Manifest.VersionType, List<ServerVersion>> vanillaDict = new();

    private WebRequestManager()
    {
    }

    public static WebRequestManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new WebRequestManager();
            }

            return instance;
        }
    }

    public async Task<List<ServerVersion>> GetVanillaVersions(Manifest.VersionType type)
    {
        return await GetVanillaVersionsFromCache(type);
    }

    public async Task<List<ServerVersion>> GetPaperVersions()
    {
        PaperWebRequester paperWebRequester = new();
        List<string> versionStrings = paperWebRequester.RequestPaperVersions();

        List<ServerVersion> versions = new();
        if (versionStrings == null)
        {
            Console.Error.WriteLine("[CRITICAL] No Paper versions found!");
            return versions;
        }

        foreach (string version in versionStrings)
        {
            ServerVersion serverVersion = new();
            serverVersion.Type = ServerVersion.VersionType.Paper;
            serverVersion.Version = version;
            int latestBuild = await paperWebRequester.RequestLatestBuildId(version);
            serverVersion.JarLink =
                $"https://api.papermc.io/v2/projects/paper/versions/{version}/builds/{latestBuild}/downloads/paper-{version}-{latestBuild}.jar";
            versions.Add(serverVersion);
        }

        return versions;
    }


    public async Task<int> GetLatestPaperBuild(string version)
    {
        return await new PaperWebRequester().RequestLatestBuildId(version);
    }

    public async Task<List<ServerVersion>> GetPurpurVersions()
    {
        return await new PurpurWebRequester().RequestPurpurVersions();
    }

    public async Task<List<ServerVersion>> GetSpigotVersions()
    {
        return await new SpigotWebRequester().RequestSpigotVersions();
    }

    public async Task<List<ServerVersion>> GetFabricVersions()
    {
        return await new FabricWebRequester().RequestFabricVersions();
    }

    private string GetJarURL(string jsonURL)
    {
        Uri uri = new(jsonURL);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        string json;
        using (WebResponse response = request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new(stream))
            json = reader.ReadToEnd();

        VersionDetails versionDetails = JsonConvert.DeserializeObject<VersionDetails>(json);

        if (versionDetails.downloads.server == null)
        {
            return null;
        }

        return versionDetails.downloads.server.url;
    }

    private void CacheVanillaVersions(Manifest.VersionType versionType, List<ServerVersion> versions)
    {
        DateTime cacheAge = DateTime.Now;

        //RAM cache
        vanillaDict[versionType] = versions;
        vanillaCacheAge[versionType] = cacheAge;
        VanillaVersionCache versionCache = new(cacheAge, versionType, versions);

        //File cache
        string json = JsonSerializer.Serialize(versionCache);
        try
        {
            string path = Path.Combine(App.ApplicationPath, "persistence");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, "vanilla-" + versionType + ".json");
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            Console.WriteLine("Error while saving Vanilla version cache. Skipping...");
        }
    }

    private async Task<List<ServerVersion>> GetVanillaVersionsFromCache(Manifest.VersionType versionType)
    {
        if (vanillaDict.ContainsKey(versionType) && VanillaCacheUpToDate(versionType))
        {
            return vanillaDict[versionType];
        }

        string path = Path.Combine(App.ApplicationPath, "persistence", "vanilla-" + versionType + ".json");
        if (File.Exists(path))
        {
            try
            {
                string json = await File.ReadAllTextAsync(path, Encoding.UTF8);
                VanillaVersionCache versionCache = JsonSerializer.Deserialize<VanillaVersionCache>(json);
                if (DateTime.Now.Subtract(versionCache.CacheCreation).TotalHours < 12)
                {
                    return versionCache.Versions;
                }

                await UpdateVanillaVersions(versionType);
                return versionCache.Versions;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("Error while reading Vanilla version cache. Skipping...");
            }
        }

        await UpdateVanillaVersions(versionType);
        return await GetVanillaVersionsFromCache(versionType);
    }

    private async Task UpdateVanillaVersions(Manifest.VersionType versionType)
    {
        Uri uri = new("https://launchermeta.mojang.com/mc/game/version_manifest.json");
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        string json;
        try
        {
            using WebResponse response = await request.GetResponseAsync();
            await using Stream stream = response.GetResponseStream();
            using StreamReader reader = new(stream);
            json = await reader.ReadToEndAsync();
        }
        catch (WebException e)
        {
            ErrorLogger.Append(e);
            Console.WriteLine(
                "WebException while updating Vanilla Versions (Either mojang.com is down or your internet coeenction is not working)");
            return;
        }


        Manifest manifest = JsonConvert.DeserializeObject<Manifest>(json);
        List<ServerVersion> result = new();
        foreach (Manifest.Version version in manifest.versions)
            if (version.type == versionType)
            {
                ServerVersion internalVersion = new();
                internalVersion.Type = versionType == Manifest.VersionType.release
                    ? ServerVersion.VersionType.Vanilla
                    : ServerVersion.VersionType.Snapshot;
                internalVersion.Version = version.id;
                internalVersion.JarLink = GetJarURL(version.url);
                if (internalVersion.JarLink != null)
                {
                    result.Add(internalVersion);
                }
            }

        CacheVanillaVersions(versionType, result);
    }

    private void UpdateVanillaVersionsAsync(Manifest.VersionType versionType)
    {
        new Thread(async () => { await UpdateVanillaVersions(versionType); }) { IsBackground = true }.Start();
    }

    private bool VanillaCacheUpToDate(Manifest.VersionType versionType)
    {
        if (vanillaCacheAge.ContainsKey(versionType))
        {
            DateTime age = vanillaCacheAge[versionType];
            DateTime now = DateTime.Now;
            TimeSpan difference = now - age;
            if (difference.TotalHours < 12)
            {
                return true;
            }
        }

        return false;
    }

    private class VanillaVersionCache
    {
        public VanillaVersionCache(DateTime cacheCreation, Manifest.VersionType versionType,
            List<ServerVersion> versions)
        {
            CacheCreation = cacheCreation;
            VersionType = versionType;
            Versions = versions;
        }

        //Empty constructor for serializers
        public VanillaVersionCache()
        {
        }

        public DateTime CacheCreation { get; set; }
        public Manifest.VersionType VersionType { get; set; }
        public List<ServerVersion> Versions { get; set; }
    }
}