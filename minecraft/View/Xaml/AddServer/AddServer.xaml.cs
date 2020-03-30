
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.xaml.AddServer
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer : Window
    {
        private AddServerViewModel viewModel = new AddServerViewModel();

        public AddServer()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            //Console.WriteLine(versionComboBox.ItemsSource);

        }

        private void ServerTypeVanilla_OnClick(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding{Source = viewModel.VanillaServerVersions});
        }
        
        private void ServerTypePaper_OnClick(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding{Source = viewModel.PaperVersions});
        }

        private void ServerTypeSpigot_OnClick(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding{Source = viewModel.SpigotServerVersions});
        }

        private async void BtnApply_OnClick(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            ServerVersion selectedVersion = (ServerVersion) versionComboBox.SelectedValue;
            string name = serverName.Text;
            //TODO check if inputs are valid / server not existing
            
            bool createServerSuccess = await ServerManager.Instance.CreateServerAsync(selectedVersion, viewModel.ServerSettings, new ServerJavaSettings(), viewModel);
            
            //TODO Do something if creating fails

            // Close the current window
            this.Close();
        }

        private void BtnRefreshVersions_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateVersions();
        }
    }
}
