using System;
using System.Collections.Generic;
using System.Windows.Controls;
using fork.Logic.Model.Settings;
using Fork.View.Xaml2.Pages.Settings;
using fork.ViewModel;

namespace fork.View.Xaml2.Pages.Settings
{
    public partial class SettingsPage : Page
    {
        private SettingsViewModel viewModel;
        
        //Pages
        private VanillaSettingsPage vanillaSettingsPage;
        private ProxySettingsPage proxySettingsPage;
        private Dictionary<SettingsFile, DefaultSettingsPage> defaultSettingsPages = new Dictionary<SettingsFile, DefaultSettingsPage>();
        
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = viewModel;
            
            
            vanillaSettingsPage = new VanillaSettingsPage(viewModel);
            proxySettingsPage = new ProxySettingsPage(viewModel);
            foreach (SettingsFile settingsFile in viewModel.Settings)
            {
                DefaultSettingsPage settingsPage = new DefaultSettingsPage(settingsFile);
                defaultSettingsPages.Add(settingsFile, settingsPage);
            }
        }

        private void FileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is SettingsFile settingsFile)
            {
                switch (settingsFile.Type)
                {
                    case SettingsFile.SettingsType.Vanilla:
                        settingsFrame.Content = vanillaSettingsPage;
                        break;
                    case SettingsFile.SettingsType.Bungee:
                        settingsFrame.Content = proxySettingsPage;
                        break;
                    default:
                        settingsFrame.Content = defaultSettingsPages[settingsFile];
                        break;
                }
            }
        }
    }
}
