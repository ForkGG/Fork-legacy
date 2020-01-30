using System.Collections.Generic;
using nihilus.Logic.Model;
using nihilus.Logic.Model.MinecraftVersionPojo;

namespace nihilus.Logic.Manager
{
    public sealed class VersionManager
    {
        private static VersionManager instance = null;

        private VersionManager() { }

        public static VersionManager Instance
        {
            get 
            {
                if (instance == null)
                    instance = new VersionManager();
                return instance;
            }
        }

        private List<ServerVersion> vanillaVersions = new List<ServerVersion>();
        private List<ServerVersion> spigotVersions = new List<ServerVersion>();


        /// <summary>
        /// The property that holds all Minecraft Vanilla Server versions
        /// </summary>
        public List<ServerVersion> VanillaVersions
        {
            get 
            {
                vanillaVersions = WebRequestManager.Instance.GetVanillaVersions(Manifest.VersionType.release);
                return vanillaVersions;
            }
        }

        /// <summary>
        /// The property that holds all Minecraft Spigot Server versions
        /// </summary>
        public List<ServerVersion> SpigotVersions
        {
            get 
            {
                spigotVersions = WebRequestManager.Instance.GetSpigotVersions();
                return spigotVersions;
            }
        }
    }
}