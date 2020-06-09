using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using fork.Logic.Model;
using fork.Logic.Model.MinecraftVersionModel;
using fork.Logic.WebRequesters;
using fork.ViewModel;

namespace fork.Logic.Manager
{
    public sealed class VersionManager
    {
        private static VersionManager instance = null;

        private VersionManager()
        {
            new Task(() =>
            {
                WaterfallVersion = new WaterfallWebRequester().RequestLatestWaterfallVersion();
            }).Start();
            new Task(() =>
            {
                BungeeCordVersion = new ServerVersion();
                BungeeCordVersion.Type = ServerVersion.VersionType.BungeeCord;
                BungeeCordVersion.Version = "";
                BungeeCordVersion.JarLink =
                    "https://ci.md-5.net/job/BungeeCord/lastSuccessfulBuild/artifact/bootstrap/target/BungeeCord.jar";
            }).Start();
            new Task(() =>
            {
                List<ServerVersion> versions =
                    WebRequestManager.Instance.GetVanillaVersions(Manifest.VersionType.release);
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => vanillaVersions.Add(version), DispatcherPriority.Background);
                }
                versions = WebRequestManager.Instance.GetPaperVersions();
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => paperVersions.Add(version));
                }

                versions = WebRequestManager.Instance.GetSpigotVersions();
                foreach (var version in versions)
                {
                    Application.Current?.Dispatcher?.InvokeAsync(() => spigotVersions.Add(version));
                }
            }).Start();
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
        private ObservableCollection<ServerVersion> spigotVersions = new ObservableCollection<ServerVersion>();
        private ObservableCollection<ServerVersion> paperVersions = new ObservableCollection<ServerVersion>();


        /// <summary>
        /// The property that holds all Minecraft Vanilla Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> VanillaVersions => vanillaVersions;

        /// <summary>
        /// The property that holds all PaperMC Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> PaperVersions => paperVersions;

        /// <summary>
        /// The property that holds all Minecraft Spigot Server versions
        /// </summary>
        public ObservableCollection<ServerVersion> SpigotVersions => spigotVersions;

        public ServerVersion WaterfallVersion;

        public ServerVersion BungeeCordVersion;
    }
}