using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Threading;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml.Pages
{
    /// <summary>
    /// Interaction logic for CreateServerPage.xaml
    /// </summary>
    public partial class CreateServerPage : Page
    {
        private AddServerViewModel viewModel;

        public event EventHandler CreateServerCloseEvent;
        
        public CreateServerPage()
        {
            viewModel = new AddServerViewModel();
            DataContext = viewModel;
            InitializeComponent();
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
            Overlay.Visibility = Visibility.Visible;
            ServerVersion selectedVersion = (ServerVersion)versionComboBox.SelectedValue;
            string name = serverName.Text;
            //TODO check if inputs are valid / server not existing

            bool createServerSuccess = await ServerManager.Instance.CreateServerAsync(selectedVersion, viewModel.ServerSettings, new ServerJavaSettings(), viewModel);

            //TODO Do something if creating fails

            // Close the page
            CreateServerCloseEvent?.Invoke(sender, e);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CreateServerCloseEvent?.Invoke(sender, e);
        }
    }
}
