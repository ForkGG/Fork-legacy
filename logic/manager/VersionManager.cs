using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Fork.Logic.Model;
using Fork.Logic.Model.MinecraftVersionModels;
using Fork.Logic.WebRequesters;
using Application = System.Windows.Application;

namespace Fork.Logic.Manager
{
    public sealed class VersionManager
    {
        private static VersionManager instance = null;

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
                    WebRequestManager.Instance.GetVanillaVersions(Manifest.VersionType.release);
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => vanillaVersions.Add(version),
                        DispatcherPriority.Background);
                }
                versions = await WebRequestManager.Instance.GetSnapshotVersions();
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => snapshotVersions.Add(version));
                }

                versions = await WebRequestManager.Instance.GetPaperVersions();
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => paperVersions.Add(version));
                }
                versions = await WebRequestManager.Instance.GetPurpurVersions();
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => purpurVersions.Add(version));
                }

                versions = await WebRequestManager.Instance.GetFabricVersions();
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => fabricVersions.Add(version));
                }
            });
        }

        public static VersionManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new VersionManager();
                return instance;
            }
        }

        private ObservableCollection<ServerVersion> vanillaVersions = new ObservableCollection<ServerVersion>();
        private ObservableCollection<ServerVersion> snapshotVersions = new ObservableCollection<ServerVersion>();
        private ObservableCollection<ServerVersion> fabricVersions = new ObservableCollection<ServerVersion>();
        private ObservableCollection<ServerVersion> paperVersions = new ObservableCollection<ServerVersion>();
        private ObservableCollection<ServerVersion> purpurVersions = new ObservableCollection<ServerVersion>();


        /// <summary>
        /// The property that holds all Minecraft Vanilla Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> VanillaVersions => vanillaVersions;

        /// <summary>
        /// The property that holds all Minecraft Snapshot Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> SnapshotVersions => snapshotVersions;

        /// <summary>
        /// The property that holds all PaperMC Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> PaperVersions => paperVersions;

        /// <summary>
        /// The property that holds all PurpurMC Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> PurpurVersions => purpurVersions;

        /// <summary>
        /// The property that holds all Minecraft Fabric Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> FabricVersions => fabricVersions;

        public ServerVersion WaterfallVersion;

        public ServerVersion BungeeCordVersion;

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
}