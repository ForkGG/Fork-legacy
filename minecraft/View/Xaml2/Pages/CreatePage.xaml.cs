using System;
using System.Collections.Generic;
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
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.ViewModel;

namespace fork.View.Xaml2.Pages
{
    /// <summary>
    /// Interaktionslogik für CreatePage.xaml
    /// </summary>
    public partial class CreatePage : Page
    {
        private AddServerViewModel viewModel;
        
        public CreatePage()
        {
            InitializeComponent();
            viewModel = new AddServerViewModel();
            DataContext = viewModel;
        }
        
        private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.VanillaServerVersions });
        }

        private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.PaperVersions });
        }

        private void ServerTypeSpigot_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.SpigotServerVersions });
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            //Overlay.Visibility = Visibility.Visible;
            ServerVersion selectedVersion = (ServerVersion)versionComboBox.SelectedValue;
            //string name = serverName.Text;
            //TODO check if inputs are valid / server not existing

            bool createServerSuccess = await ServerManager.Instance.CreateServerAsync(selectedVersion, viewModel.ServerSettings, new ServerJavaSettings(), viewModel);

            //TODO Do something if creating fails

            // Close the page
            //CreateServerCloseEvent?.Invoke(sender, e);
        }
    }
}
