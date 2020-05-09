using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using fork.ViewModel;

namespace fork.View.Xaml2.Pages.Settings
{
    /// <summary>
    /// Interaktionslogik für SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private SettingsViewModel viewModel;
        HashSet<Frame> subPages = new HashSet<Frame>();
        
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = viewModel;
            subPages.Add(mcSettings);
            subPages.Add(bukkitSettings);
            subPages.Add(spigotSettings);
        }

        private void SelectMCSettings(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            if (mcSettings != null)
            {
                mcSettings.Visibility = Visibility.Visible;
            }
        }
        
        private void SelectBukkitSettings(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            bukkitSettings.Visibility = Visibility.Visible;
        }
        private void SelectSpigotSettings(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            spigotSettings.Visibility = Visibility.Visible;
        }

        private void HideAllPages()
        {
            foreach (Frame frame in subPages)
            {
                frame.Visibility = Visibility.Hidden;
            }
        }
    }
}
