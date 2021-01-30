using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.PluginModels;
using Fork.Logic.Model.ProxyModels;
using Fork.View.Xaml2.Controls;
using Fork.ViewModel;

namespace Fork.View.Resources.dictionaries
{
    public partial class ListViewStyles : ResourceDictionary
    {

        private void OpenServer_Click(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            NetworkForkServer networkForkServer = textBlock.DataContext as NetworkForkServer;
            ApplicationManager.Instance.MainViewModel.SelectedEntity = networkForkServer.ServerViewModel;
        }

        private void RemoveFromNetwork_Click(object sender, RoutedEventArgs e)
        {
            IconButton button = sender as IconButton;
            NetworkViewModel viewModel = button.CommandParameter as NetworkViewModel;
            viewModel?.RemoveServer(button.DataContext as NetworkServer);
        }

        private void RemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            IconButton button = sender as IconButton;
            NetworkViewModel viewModel = button.CommandParameter as NetworkViewModel;
            viewModel?.RemovePermission(button.DataContext as Permission);
        }
        
        private void RemoveUser_Click(object sender, RoutedEventArgs e)
        {
            IconButton button = sender as IconButton;
            NetworkViewModel viewModel = button.CommandParameter as NetworkViewModel;
            viewModel?.RemoveGroup(button.DataContext as Group);
        }

        private void VisitPlugin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Plugin plugin)
                {
                    string pluginUrl = plugin.file.url.Substring(0, 
                        plugin.file.url.IndexOf("/download?", StringComparison.Ordinal));
                    string url = "https://www.spigotmc.org/"+pluginUrl;
                    //hack for windows only https://github.com/dotnet/corefx/issues/10361
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
            }
        }

        private async void InstallPlugin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is IconButton iconButton)
            {
                if (iconButton.CommandParameter is PluginViewModel pluginViewModel && iconButton.DataContext is Plugin plugin)
                {
                    bool result = await PluginManager.Instance.InstallPluginAsync(plugin, pluginViewModel);
                }
            }
        }
    }
}
