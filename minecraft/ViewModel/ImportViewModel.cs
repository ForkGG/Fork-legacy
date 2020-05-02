using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows.Controls;
using nihilus.Logic.ImportLogic;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.View.Xaml.Pages;
using nihilus.View.Xaml.Pages.ImportPages;

namespace nihilus.ViewModel
{
    public class ImportViewModel : BaseViewModel
    {
        public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; } 
        public ObservableCollection<ServerVersion> PaperVersions { get; set; }
        public ObservableCollection<ServerVersion> SpigotServerVersions { get; set; }
        public ServerSettings ServerSettings { get; set; }
        public double DownloadProgress { get; set; }
        public string DownloadProgressReadable { get; set; }
        public bool DownloadCompleted { get; set; }
        public double CopyProgress { get; set; }
        public string CopyProgressReadable { get; set; }
        public bool CopyCompleted { get; set; }

        public event EventHandler ImportNextEvent;
        public event EventHandler ImportPreviousEvent;
        public event EventHandler ImportCloseEvent;

        /// <summary>
        /// Constructor
        /// Sets ServerVersion to the currently existing server versions
        /// </summary>
        public ImportViewModel()
        {
            VanillaServerVersions = VersionManager.Instance.VanillaVersions;
            PaperVersions = VersionManager.Instance.PaperVersions;
            SpigotServerVersions = VersionManager.Instance.SpigotVersions;
            ServerSettings = new ServerSettings(ServerManager.Instance.NextDefaultServerName());
        }

        public void RegenerateServerSettings()
        {
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

        public void CopyProgressChanged(object sender, FileImporter.CopyProgressChangedEventArgs e)
        {
            CopyProgress = (double)e.FilesCopied / (double)e.FilesToCopy *100;
            CopyProgressReadable = Math.Round(CopyProgress, 0) + "%";
        }

        public void RaiseImportNextEvent(object sender, EventArgs e)
        {
            ImportNextEvent?.Invoke(sender, e);
        }
        public void RaiseImportPreviousEvent(object sender, EventArgs e)
        {
            ImportPreviousEvent?.Invoke(sender, e);
        }
        public void RaiseImportCloseEvent(object sender, EventArgs e)
        {
            ImportCloseEvent?.Invoke(sender, e);
        }
    }
}