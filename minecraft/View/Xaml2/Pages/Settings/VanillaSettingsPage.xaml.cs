using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml2.Pages.Settings
{
    /// <summary>
    /// Interaktionslogik für MCSettingsPage.xaml
    /// </summary>
    public partial class VanillaSettingsPage : Page
    {
        private SettingsViewModel viewModel;
        
        public VanillaSettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        private void VersionChange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((bool) !versionComboBox.SelectedItem?.Equals(viewModel.Server.Version))
            {
                VersionChangeBtn.Visibility = Visibility.Visible;
            }
            else
            {
                VersionChangeBtn.Visibility = Visibility.Collapsed; 
            }
        }
    }
}
