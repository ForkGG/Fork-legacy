using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using fork.Logic.Manager;
using fork.Logic.Model;

namespace fork.ViewModel
{
    
    public class AddServerViewModel : BaseViewModel
    {

        public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; } 
        public ObservableCollection<ServerVersion> PaperVersions { get; set; }
        public ObservableCollection<ServerVersion> SpigotServerVersions { get; set; }
        public ServerSettings ServerSettings { get; set; }
        public bool DownloadCompleted { get; set; }

        /// <summary>
        /// Constructor
        /// Sets ServerVersion to the currently existing server versions
        /// </summary>
        public AddServerViewModel()
        {
            VanillaServerVersions = VersionManager.Instance.VanillaVersions;
            PaperVersions = VersionManager.Instance.PaperVersions;
            SpigotServerVersions = VersionManager.Instance.SpigotVersions;
            GenerateNewSettings();
        }

        public void GenerateNewSettings()
        {
            ServerSettings = new ServerSettings("world");
        }
    }
}
