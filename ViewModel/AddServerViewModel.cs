using System.Collections.ObjectModel;
using Fork.Logic.Manager;
using Fork.Logic.Model;

namespace Fork.ViewModel
{
    public class AddServerViewModel : BaseViewModel
    {
        /// <summary>
        ///     Constructor
        ///     Sets ServerVersion to the currently existing server versions
        /// </summary>
        public AddServerViewModel()
        {
            VanillaServerVersions = VersionManager.Instance.VanillaVersions;
            PaperVersions = VersionManager.Instance.PaperVersions;
            SpigotServerVersions = VersionManager.Instance.SpigotVersions;
            GenerateNewSettings();
        }

        public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; }
        public ObservableCollection<ServerVersion> PaperVersions { get; set; }
        public ObservableCollection<ServerVersion> SpigotServerVersions { get; set; }
        public ServerSettings ServerSettings { get; set; }
        public bool DownloadCompleted { get; set; }

        public void GenerateNewSettings()
        {
            ServerSettings = new ServerSettings("world");
        }
    }
}