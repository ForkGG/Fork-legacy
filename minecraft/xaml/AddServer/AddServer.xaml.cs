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
using System.Windows.Shapes;
using nihilus.logic.managers;
using nihilus.logic.model;
using nihilus.logic.ViewModel;

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
            
            ServerManager.Instance.CreateServer(selectedVersion, viewModel.ServerSettings, new ServerJavaSettings());
            
            // Close the current window
            this.Close();
        }

        private void BtnRefreshVersions_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateVersions();
        }
    }
}
