using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using Fork.Logic.ImportLogic;
using Fork.Logic.Manager;
using Fork.Logic.Model;

namespace Fork.ViewModel;

public class ImportViewModel : BaseViewModel
{
    /// <summary>
    ///     Constructor
    ///     Sets ServerVersion to the currently existing server versions
    /// </summary>
    public ImportViewModel()
    {
        VanillaServerVersions = VersionManager.Instance.VanillaVersions;
        SnapshotServerVersions = VersionManager.Instance.SnapshotVersions;
        PaperVersions = VersionManager.Instance.PaperVersions;
        PurpurVersions = VersionManager.Instance.PurpurVersions;
        SpigotVersions = VersionManager.Instance.SpigotVersions;
        FabricServerVersions = VersionManager.Instance.FabricVersions;
        ServerSettings = new ServerSettings("world");
    }

    public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; }
    public ObservableCollection<ServerVersion> SnapshotServerVersions { get; set; }
    public ObservableCollection<ServerVersion> PaperVersions { get; set; }
    public ObservableCollection<ServerVersion> PurpurVersions { get; set; }
    public ObservableCollection<ServerVersion> SpigotVersions { get; set; }
    public ObservableCollection<ServerVersion> FabricServerVersions { get; set; }
    public ServerSettings ServerSettings { get; set; }
    public double DownloadProgress { get; set; }
    public string DownloadProgressReadable { get; set; }
    public bool DownloadCompleted { get; set; }
    public double CopyProgress { get; set; }
    public string CopyProgressReadable { get; set; }
    public bool CopyCompleted { get; set; }

    public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        double bytesIn = double.Parse(e.BytesReceived.ToString());
        double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
        DownloadProgress = bytesIn / totalBytes * 100;
        DownloadProgressReadable = Math.Round(DownloadProgress, 0) + "%";
    }

    public void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs e)
    {
        DownloadCompleted = true;
    }

    public void CopyProgressChanged(object sender, FileImporter.CopyProgressChangedEventArgs e)
    {
        CopyProgress = e.FilesCopied / (double)e.FilesToCopy * 100;
        CopyProgressReadable = Math.Round(CopyProgress, 0) + "%";
    }
}