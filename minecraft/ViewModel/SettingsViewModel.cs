using System;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.View.Xaml2.Pages.Settings;

namespace nihilus.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        #region private Varibles
        
        

        #endregion

        #region Properties
        
        //Others
        public Server Server { get; set; }
        public ObservableCollection<ServerVersion> Versions { get; set; }

        //Pages
        public SettingsPage SettingsPage { get; }
        public VanillaSettingsPage VanillaSettingsPage { get; }

        #endregion

        public SettingsViewModel(Server server)
        {
            Server = server;
            //Server = new Server("test", new ServerVersion(), new ServerSettings("test"), new ServerJavaSettings());
            SettingsPage = new SettingsPage(this);
            VanillaSettingsPage = new VanillaSettingsPage(this);
            switch (Server.Version.Type)
            {
                case ServerVersion.VersionType.Vanilla:
                    Versions = VersionManager.Instance.VanillaVersions;
                    break;
                case ServerVersion.VersionType.Paper:
                    Versions = VersionManager.Instance.PaperVersions;
                    break;
                default:
                    throw new Exception("SettingsViewModel constructor does not implement this version type!");
            }
        }
    }
}