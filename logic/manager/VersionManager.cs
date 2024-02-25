using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using Fork.Logic.Model;
using Fork.Logic.Model.MinecraftVersionModels;
using Fork.Logic.WebRequesters;
using Application = System.Windows.Application;

namespace Fork.Logic.Manager;

public sealed class VersionManager
{
    private static VersionManager instance;

    public ServerVersion BungeeCordVersion;

    public ServerVersion WaterfallVersion;

    private VersionManager()
    {
        Task.Run(async () => { WaterfallVersion = await new WaterfallWebRequester().RequestLatestWaterfallVersion(); });
        Task.Run(() =>
        {
            BungeeCordVersion = new ServerVersion();
            BungeeCordVersion.Type = ServerVersion.VersionType.BungeeCord;
            BungeeCordVersion.Version = "";
            BungeeCordVersion.JarLink =
                "https://ci.md-5.net/job/BungeeCord/lastSuccessfulBuild/artifact/bootstrap/target/BungeeCord.jar";
        });
        Task.Run(async () =>
        {
            List<ServerVersion> versions =
                await WebRequestManager.Instance.GetVanillaVersions(Manifest.VersionType.release);
            foreach (ServerVersion version in versions)
                Application.Current?.Dispatcher?.InvokeAsync(() => VanillaVersions.Add(version),
                    DispatcherPriority.Background);
            versions = await WebRequestManager.Instance.GetVanillaVersions(Manifest.VersionType.snapshot);
            foreach (ServerVersion version in versions)
                Application.Current?.Dispatcher?.InvokeAsync(() => SnapshotVersions.Add(version));

            versions = await WebRequestManager.Instance.GetPaperVersions();
            foreach (ServerVersion version in versions)
                Application.Current?.Dispatcher?.InvokeAsync(() => PaperVersions.Add(version));

            versions = await WebRequestManager.Instance.GetPurpurVersions();
            foreach (ServerVersion version in versions)
                Application.Current?.Dispatcher?.InvokeAsync(() => PurpurVersions.Add(version));

            versions = await WebRequestManager.Instance.GetSpigotVersions();
            foreach (ServerVersion version in versions)
                Application.Current?.Dispatcher?.InvokeAsync(() => SpigotVersions.Add(version));

            versions = await WebRequestManager.Instance.GetFabricVersions();
            foreach (ServerVersion version in versions)
                Application.Current?.Dispatcher?.InvokeAsync(() => FabricVersions.Add(version));
        });
    }

    public static VersionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new VersionManager();
            }

            return instance;
        }
    }


    /// <summary>
    ///     The property that holds all Minecraft Vanilla Server versions
    /// </summary>
    public ObservableCollection<ServerVersion> VanillaVersions { get; } = new();

    /// <summary>
    ///     The property that holds all Minecraft Snapshot Server versions
    /// </summary>
    public ObservableCollection<ServerVersion> SnapshotVersions { get; } = new();

    /// <summary>
    ///     The property that holds all PaperMC Server versions
    /// </summary>
    public ObservableCollection<ServerVersion> PaperVersions { get; } = new();

    /// <summary>
    ///     The property that holds all PurpurMC Server versions
    /// </summary>
    public ObservableCollection<ServerVersion> PurpurVersions { get; } = new();

    /// <summary>
    ///     The property that holds all Minecraft Spigot Server versions
    /// </summary>
    public ObservableCollection<ServerVersion> SpigotVersions { get; } = new();

    /// <summary>
    ///     The property that holds all Minecraft Fabric Server versions
    /// </summary>
    public ObservableCollection<ServerVersion> FabricVersions { get; } = new();

    public async Task<int> GetLatestBuild(ServerVersion version)
    {
        if (!version.SupportBuilds)
        {
            return 0;
        }

        switch (version.Type)
        {
            case ServerVersion.VersionType.Paper:
                return await WebRequestManager.Instance.GetLatestPaperBuild(version.Version);
            default:
                return 0;
        }
    }
}