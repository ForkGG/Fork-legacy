using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using nihilus.Logic.Model;
using nihilus.Logic.Model.MinecraftVersionPojo;
using nihilus.ViewModel;

namespace nihilus.Logic.Manager
{
    public sealed class VersionManager
    {
        private static VersionManager instance = null;

        private VersionManager()
        {
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
    }
}