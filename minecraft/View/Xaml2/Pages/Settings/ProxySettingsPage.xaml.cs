using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Fork.Logic.Model.ProxyModels;
using fork.Logic.Model.Settings;
using fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Settings
{
    /// <summary>
    /// Interaktionslogik für ProxySettingsPage.xaml
    /// </summary>
    public partial class ProxySettingsPage : Page
    {
        private NetworkViewModel networkViewModel;
        
        public ProxySettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            networkViewModel = viewModel.EntityViewModel as NetworkViewModel;
            DataContext = networkViewModel;
        }

        private void AddExternalServer_Click(object sender, MouseButtonEventArgs e)
        {
            AddOverlay.Visibility = Visibility.Visible;
        }
        
        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            AddGroupOverlay.Visibility = Visibility.Visible;
        }
        
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            AddUserOverlay.Visibility = Visibility.Visible;
        }

        private void AddServerCancel_Click(object sender, RoutedEventArgs e)
        {
            AddOverlay.Visibility = Visibility.Collapsed;
            newServerAddress.Clear();
            newServerName.Clear();
            newServerMotd.Clear();
        }

        private void AddServerApply_Click(object sender, RoutedEventArgs e)
        {
            string name = newServerName.Text;
            fork.Logic.Model.Settings.Server server = new fork.Logic.Model.Settings.Server
            {
                address = newServerAddress.Text,
                motd = newServerMotd.Text,
                forkServer = false,
                restricted = false
            };
            networkViewModel.AddServer(server, name);
            AddServerCancel_Click(sender, e);
        }
        
        private void AddGroupCancel_Click(object sender, RoutedEventArgs e)
        {
            AddGroupOverlay.Visibility = Visibility.Collapsed;
            newGroupName.Clear();
        }

        private void AddGroupApply_Click(object sender, RoutedEventArgs e)
        {
            string name = newGroupName.Text;
            Permission permission = new Permission(name, new ObservableCollection<string>()); 
            networkViewModel.AddPermission(permission);
            AddGroupCancel_Click(sender, e);
        }
        
        private void AddUserCancel_Click(object sender, RoutedEventArgs e)
        {
            AddUserOverlay.Visibility = Visibility.Collapsed;
            newUserName.Clear();
        }

        private void AddUserApply_Click(object sender, RoutedEventArgs e)
        {
            string name = newUserName.Text;
            Group group = new Group(name, new ObservableCollection<string>()); 
            networkViewModel.AddGroup(group);
            AddUserCancel_Click(sender, e);
        }
    }
}
