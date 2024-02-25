using System.Collections.ObjectModel;
using Fork.Logic.Manager;
using Fork.Logic.Model;

namespace Fork.ViewModel;

public class AddServerViewModel : BaseViewModel
{
    /// <summary>
    ///     Constructor
    ///     Sets ServerVersion to the currently existing server versions
    /// </summary>
    public AddServerViewModel()
    {
        VanillaServerVersions = VersionManager.Instance.VanillaVersions;
        SnapshotServerVersions = VersionManager.Instance.SnapshotVersions;
        PaperVersions = VersionManager.Instance.PaperVersions;
        PurpurVersions = VersionManager.Instance.PurpurVersions;
        SpigotServerVersions = VersionManager.Instance.SpigotVersions;
        FabricServerVersions = VersionManager.Instance.FabricVersions;
        GenerateNewSettings();
    }

    public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; }
    public ObservableCollection<ServerVersion> SnapshotServerVersions { get; set; }
    public ObservableCollection<ServerVersion> PaperVersions { get; set; }
    public ObservableCollection<ServerVersion> PurpurVersions { get; set; }
    public ObservableCollection<ServerVersion> FabricServerVersions { get; set; }
    public ObservableCollection<ServerVersion> SpigotServerVersions { get; set; }
    public ServerSettings ServerSettings { get; set; }
    public bool DownloadCompleted { get; set; }

    public void GenerateNewSettings()
    {
        ServerSettings = new ServerSettings("world");
    }
}