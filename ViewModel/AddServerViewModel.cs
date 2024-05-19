using System.Collections.ObjectModel;
using System.Windows;
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
        Application.Current?.Dispatcher?.InvokeAsync(() =>
        {
            VanillaVersionsLoading = VanillaServerVersions.Count == 0;
            VersionManager.Instance.VanillaVersions.CollectionChanged +=
                (sender, args) =>
                {
                    VanillaVersionsLoading = VanillaServerVersions.Count == 0;
                    RaisePropertyChanged(sender, nameof(VanillaVersionsLoading));
                };
            SnapshotVersionsLoading = SnapshotServerVersions.Count == 0;
            VersionManager.Instance.SnapshotVersions.CollectionChanged +=
                (sender, args) =>
                {
                    SnapshotVersionsLoading = SnapshotServerVersions.Count == 0;
                    RaisePropertyChanged(sender, nameof(SnapshotVersionsLoading));
                };
            PaperVersionsLoading = PaperVersions.Count == 0;
            VersionManager.Instance.PaperVersions.CollectionChanged +=
                (sender, args) =>
                {
                    PaperVersionsLoading = PaperVersions.Count == 0;
                    RaisePropertyChanged(sender, nameof(PaperVersionsLoading));
                };
            PurpurVersionsLoading = PurpurVersions.Count == 0;
            VersionManager.Instance.PurpurVersions.CollectionChanged +=
                (sender, args) =>
                {
                    PurpurVersionsLoading = PurpurVersions.Count == 0;
                    RaisePropertyChanged(sender, nameof(PurpurVersionsLoading));
                };
            FabricVersionsLoading = FabricServerVersions.Count == 0;
            VersionManager.Instance.FabricVersions.CollectionChanged +=
                (sender, args) =>
                {
                    FabricVersionsLoading = FabricServerVersions.Count == 0;
                    RaisePropertyChanged(sender, nameof(FabricVersionsLoading));
                };
            SpigotVersionsLoading = SpigotServerVersions.Count == 0;
            VersionManager.Instance.SpigotVersions.CollectionChanged +=
                (sender, args) =>
                {
                    SpigotVersionsLoading = SpigotServerVersions.Count == 0;
                    RaisePropertyChanged(sender, nameof(SpigotVersionsLoading));
                };
        });
    }

    public ObservableCollection<ServerVersion> VanillaServerVersions { get; init; }
    public bool VanillaVersionsLoading { get; private set; }
    public ObservableCollection<ServerVersion> SnapshotServerVersions { get; init; }
    public bool SnapshotVersionsLoading { get; private set; }
    public ObservableCollection<ServerVersion> PaperVersions { get; init; }
    public bool PaperVersionsLoading { get; private set; }
    public ObservableCollection<ServerVersion> PurpurVersions { get; init; }
    public bool PurpurVersionsLoading { get; private set; }
    public ObservableCollection<ServerVersion> FabricServerVersions { get; init; }
    public bool FabricVersionsLoading { get; private set; }
    public ObservableCollection<ServerVersion> SpigotServerVersions { get; init; }
    public bool SpigotVersionsLoading { get; private set; }
    public ServerSettings ServerSettings { get; private set; }
    public bool DownloadCompleted { get; set; }

    public void GenerateNewSettings()
    {
        ServerSettings = new ServerSettings("world");
    }
}