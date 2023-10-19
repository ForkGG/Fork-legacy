﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using Fork.Logic.Manager;
using Fork.Logic.Model;

namespace Fork.ViewModel
{
    
    public class AddServerViewModel : BaseViewModel
    {

        public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; } 
        public ObservableCollection<ServerVersion> SnapshotServerVersions { get; set; } 
        public ObservableCollection<ServerVersion> PaperVersions { get; set; }
        public ObservableCollection<ServerVersion> PurpurVersions { get; set; }
        public ObservableCollection<ServerVersion> FabricServerVersions { get; set; }
        public ServerSettings ServerSettings { get; set; }
        public bool DownloadCompleted { get; set; }

        /// <summary>
        /// Constructor
        /// Sets ServerVersion to the currently existing server versions
        /// </summary>
        public AddServerViewModel()
        {
            VanillaServerVersions = VersionManager.Instance.VanillaVersions;
            SnapshotServerVersions = VersionManager.Instance.SnapshotVersions;
            PaperVersions = VersionManager.Instance.PaperVersions;
            PurpurVersions = VersionManager.Instance.PurpurVersions;
            FabricServerVersions = VersionManager.Instance.FabricVersions;
            GenerateNewSettings();
        }

        public void GenerateNewSettings()
        {
            ServerSettings = new ServerSettings("world");
        }
    }
}
