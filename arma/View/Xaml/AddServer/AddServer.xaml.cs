
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
            Console.WriteLine(versionComboBox.ItemsSource);

        }

        private void ServerTypeVanilla_OnClick(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding{Source = viewModel.VanillaServerVersions});
        }

        private void ServerTypeSpigot_OnClick(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding{Source = viewModel.SpigotServerVersions});
        }

        private void BtnApply_OnClick(object sender, RoutedEventArgs e)
        {
            ServerVersion selectedVersion = (ServerVersion) versionComboBox.SelectedValue;
            string name = serverName.Text;
            //TODO check if inputs are valid / server not existing
            
            ServerManager.Instance.CreateServer(selectedVersion, viewModel.ServerSettings);
            
            // Close the current window
            this.Close();
        }

        private void BtnRefreshVersions_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateVersions();
        }
    }
}
