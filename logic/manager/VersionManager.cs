using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.Logic.Model.MinecraftVersionModels;
using Fork.logic.Utils;
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
            try
            {
                List<ServerVersion> vanillaVersions =
                    await WebRequestManager.Instance.GetVanillaVersions(Manifest.VersionType.release);
                Application.Current?.Dispatcher?.InvokeAsync(() => VanillaVersions.AddRange(vanillaVersions));
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        });
        Task.Run(async () =>
        {
            try
            {
                List<ServerVersion> snapshotVersions =
                    await WebRequestManager.Instance.GetVanillaVersions(Manifest.VersionType.snapshot);
                Application.Current?.Dispatcher?.InvokeAsync(() => SnapshotVersions.AddRange(snapshotVersions));
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        });
        Task.Run(async () =>
        {
            try
            {
                List<ServerVersion> paperVersions = await WebRequestManager.Instance.GetPaperVersions();
                Application.Current?.Dispatcher?.InvokeAsync(() => PaperVersions.AddRange(paperVersions));
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        });
        Task.Run(async () =>
        {
            try
            {
                List<ServerVersion> purpurVersions = await WebRequestManager.Instance.GetPurpurVersions();
                Application.Current?.Dispatcher?.InvokeAsync(() => PurpurVersions.AddRange(purpurVersions));
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        });
        Task.Run(async () =>
        {
            try
            {
                List<ServerVersion> spigotVersions = await WebRequestManager.Instance.GetSpigotVersions();
                Application.Current?.Dispatcher?.InvokeAsync(() => SpigotVersions.AddRange(spigotVersions));
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
        });
        Task.Run(async () =>
        {
            try
            {
                List<ServerVersion> fabricVersions = await WebRequestManager.Instance.GetFabricVersions();
                Application.Current?.Dispatcher?.InvokeAsync(() => FabricVersions.AddRange(fabricVersions));
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
            }
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
    public ObservableRangeCollection<ServerVersion> VanillaVersions { get; } = new();

    /// <summary>
    ///     The property that holds all Minecraft Snapshot Server versions
    /// </summary>
    public ObservableRangeCollection<ServerVersion> SnapshotVersions { get; } = new();

    /// <summary>
    ///     The property that holds all PaperMC Server versions
    /// </summary>
    public ObservableRangeCollection<ServerVersion> PaperVersions { get; } = new();

    /// <summary>
    ///     The property that holds all PurpurMC Server versions
    /// </summary>
    public ObservableRangeCollection<ServerVersion> PurpurVersions { get; } = new();

    /// <summary>
    ///     The property that holds all Minecraft Spigot Server versions
    /// </summary>
    public ObservableRangeCollection<ServerVersion> SpigotVersions { get; } = new();

    /// <summary>
    ///     The property that holds all Minecraft Fabric Server versions
    /// </summary>
    public ObservableRangeCollection<ServerVersion> FabricVersions { get; } = new();

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