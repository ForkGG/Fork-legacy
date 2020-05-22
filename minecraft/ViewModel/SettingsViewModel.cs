using System;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.View.Xaml2.Pages.Settings;

namespace fork.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        #region private Varibles
        
        

        #endregion

        #region Properties
        
        //Others
        public Server Server { get; set; }
        public ObservableCollection<ServerVersion> Versions { get; set; }
        public ServerViewModel ServerViewModel { get; set; }

        //Pages
        public SettingsPage SettingsPage { get; }
        public VanillaSettingsPage VanillaSettingsPage { get; }

        #endregion

        public SettingsViewModel(ServerViewModel serverViewModel)
        {
            ServerViewModel = serverViewModel;
            Server = serverViewModel.Server;
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