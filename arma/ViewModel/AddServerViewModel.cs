using System.Collections.ObjectModel;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;

namespace nihilus.ViewModel
{
    
    class AddServerViewModel : BaseViewModel
    {

        public ObservableCollection<ServerVersion> VanillaServerVersions { get; set; } 
        public ObservableCollection<ServerVersion> SpigotServerVersions { get; set; }
        public ServerSettings ServerSettings { get; set; }

        /// <summary>
        /// Constructor
        /// Sets ServerVersion to the currently existing server versions
        /// </summary>
        public AddServerViewModel()
        {
            VanillaServerVersions = new ObservableCollection<ServerVersion>(VersionManager.Instance.VanillaVersions);
            SpigotServerVersions = new ObservableCollection<ServerVersion>(VersionManager.Instance.SpigotVersions);
            ServerSettings = new ServerSettings(ServerManager.Instance.NextDefaultServerName());
        }

        public void UpdateVersions()
        {
            VanillaServerVersions = new ObservableCollection<ServerVersion>(VersionManager.Instance.VanillaVersions);
            SpigotServerVersions = new ObservableCollection<ServerVersion>(VersionManager.Instance.SpigotVersions);
            SpigotServerVersions.Add(new ServerVersion());
        }
    }
}
