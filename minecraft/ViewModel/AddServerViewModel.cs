using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;

namespace nihilus.ViewModel
{
    
    public class AddServerViewModel : BaseViewModel
    {

        public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; } 
        public ObservableCollection<ServerVersion> PaperVersions { get; set; }
        public ObservableCollection<ServerVersion> SpigotServerVersions { get; set; }
        public ServerSettings ServerSettings { get; set; }
        public double DownloadProgress { get; set; }
        public string DownloadProgressReadable { get; set; }
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
            ServerSettings = new ServerSettings(ServerManager.Instance.NextDefaultServerName());
        }

        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            DownloadProgress = bytesIn / totalBytes * 100;
            DownloadProgressReadable = Math.Round(DownloadProgress, 0)+"%";
        }

        public void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs e)
        {
            DownloadCompleted = true;
        }
    }
}
