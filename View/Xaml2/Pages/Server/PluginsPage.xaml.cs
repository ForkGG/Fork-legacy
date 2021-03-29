using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Fork.Logic.Manager;
using Fork.logic.model.PluginModels;
using Fork.View.Xaml2.Controls;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Server
{
    public partial class PluginsPage : Page
    {
        private PluginViewModel viewModel;
        private bool isLoadingMore = false;
        
        public PluginsPage(PluginViewModel pluginViewModel)
        {
            viewModel = pluginViewModel;
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OpenExplorer_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(App.ServerPath, viewModel.EntityViewModel.Entity.Name);
            Process.Start("explorer.exe", "-p, " + path);
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadList();
        }

        private async void ReloadList()
        {
            searchBox.IsEnabled = false;
            categoryBox.IsEnabled = false;
            sortBox.IsEnabled = false;
            loadingOverlay.Visibility = Visibility.Visible;

            pluginScrollViewer.ScrollToTop();
            await viewModel.Reload();
            
            searchBox.IsEnabled = true;
            categoryBox.IsEnabled = true;
            sortBox.IsEnabled = true;
            loadingOverlay.Visibility = Visibility.Collapsed;
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var fullHeight = e.ExtentHeight;
            var currTop = e.VerticalOffset;
            var viewableHeight = e.ViewportHeight;

            if (!isLoadingMore && currTop + viewableHeight*2 >= fullHeight)
            {
                isLoadingMore = true;
                await viewModel.LoadMore();
                isLoadingMore = false;
            }
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                searchBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                ReloadList();
            }
        }

        private async void DeletePluginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is IconButton iconButton)
            {
                if (iconButton.CommandParameter is InstalledPlugin installedPlugin)
                {
                    //TODO implement error indicator
                    bool result = await PluginManager.Instance.DeletePluginAsync(installedPlugin, viewModel);
                }
            }
        }

        private async void EnableDisablePlugin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                checkBox.IsEnabled = false;
                if (checkBox.CommandParameter is InstalledPlugin installedPlugin)
                {
                    bool result;
                    if (installedPlugin.IsEnabled)
                    {
                        result = await PluginManager.Instance.DisablePluginAsync(installedPlugin, viewModel);
                    }
                    else
                    {
                        result = await PluginManager.Instance.EnablePluginAsync(installedPlugin, viewModel);
                    }
                    //TODO show error if result=false
                }
                checkBox.IsEnabled = true;
            }
        }
    }
}
