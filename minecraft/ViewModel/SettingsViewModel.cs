using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms.VisualStyles;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Model.Settings;
using fork.View.Xaml2.Pages.Settings;

namespace fork.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        #region private Varibles
        
        

        #endregion

        #region Properties
        
        //Others
        public Entity Entity { get; set; }
        public ObservableCollection<SettingsFile> Settings { get; set; }
        public EntityViewModel EntityViewModel { get; set; }

        //Pages
        public SettingsPage SettingsPage { get; }

        #endregion

        public SettingsViewModel(ServerViewModel serverViewModel, List<SettingsFile> settings)
        {
            EntityViewModel = serverViewModel;
            Settings = new ObservableCollection<SettingsFile>(settings);
            Entity = serverViewModel.Entity;
            SettingsPage = new SettingsPage(this);
            //VanillaSettingsPage = new VanillaSettingsPage(this);
        }

        public SettingsViewModel(NetworkViewModel networkViewModel, List<SettingsFile> settings)
        {
            EntityViewModel = networkViewModel;
            Settings = new ObservableCollection<SettingsFile>(settings);
            Entity = networkViewModel.Network;
            SettingsPage = new SettingsPage(this);
        }
    }
}