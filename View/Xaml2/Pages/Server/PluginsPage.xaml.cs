using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fork.Logic.Manager;
using Fork.logic.model.PluginModels;
using Fork.View.Xaml2.Controls;
using Fork.ViewModel;
using Fork.Logic.Logging;
using System;
using ICSharpCode.SharpZipLib.Zip;
using YamlDotNet.Serialization;
using Fork.Logic.Utils;
using Fork.Logic.Persistence;
using System.Linq;
using Fork.Logic.Model;
using Fork.Logic.Model.ServerConsole;
using System.Threading.Tasks;
using Fork.logic.Utils;

namespace Fork.View.Xaml2.Pages.Server
{
    public partial class PluginsPage : Page
    {
        public PluginViewModel viewModel;
        private bool isLoadingMore = false;
        
        public PluginsPage(PluginViewModel pluginViewModel)
        {
            viewModel = pluginViewModel;
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void InstallLocal_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string downloadsDirectory = null;
            try
            {
                downloadsDirectory = System.Convert.ToString(
                    Microsoft.Win32.Registry.GetValue(
                         @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"
                        , "{374DE290-123F-4565-9164-39C4925E467B}"
                        , String.Empty
                    )
                );
            } catch (Exception) { }

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Jar file|*.jar",
                Multiselect = true
            };
            if (downloadsDirectory != null)
            {
                dialog.InitialDirectory = downloadsDirectory;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = Path.Combine(App.ServerPath, viewModel.EntityViewModel.Entity.Name, "plugins");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string[] selected = dialog.FileNames;

                foreach (string selectedElement in selected)
                {
                    string pluginName = LocalPluginUtils.ReadPluginName(selectedElement);
                    string fileName = Path.GetFileName(selectedElement);
                    string targetName = pluginName ?? StringUtils.BeautifyPluginName(fileName[..^4]);

                    string destination = Path.Combine(path, $"{targetName}.jar");

                    try
                    {
                        File.Copy(selectedElement, destination, true);

                        if (pluginName != null)
                        {
                            bool notInstall = false;
                            foreach (InstalledPlugin iPlugin in viewModel.InstalledPlugins)
                            {
                                string iName;
                                if (iPlugin.Plugin == null)
                                {
                                    iName = iPlugin.Name;
                                }
                                else
                                {
                                    iName = iPlugin.Plugin.name;
                                }

                                if (iName.Equals(pluginName))
                                {
                                    notInstall = true;
                                    break;
                                }
                            }

                            if (notInstall)
                            {
                                viewModel.EntityViewModel.AddToConsole(
                                    new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Not installing {pluginName} because it's already installed", ConsoleMessage.MessageLevel.WARN));
                                return;
                            }

                            Console.WriteLine($"Checking data folder for {pluginName}");
                            string pluginDataFolder = Path.Combine(selectedElement[..^fileName.Length], pluginName);
                            string localData = Path.Combine(path, pluginName);

                            try
                            {
                                if (Directory.Exists(pluginDataFolder) && !Directory.Exists(localData))
                                {
                                    Directory.CreateDirectory(localData);
                                    viewModel.EntityViewModel.AddToConsole(
                                        new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Copying {pluginDataFolder} to plugins/{pluginName}", ConsoleMessage.MessageLevel.INFO));
                                    LocalPluginUtils.CopyFilesRecursively(pluginDataFolder, localData);
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.Append(ex);
                                viewModel.EntityViewModel.AddToConsole(
                                        new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Failed to copy directory from {pluginDataFolder} to plugins/{pluginName}", ConsoleMessage.MessageLevel.ERROR));
                            }
                        }
                        else
                        {
                            viewModel.EntityViewModel.AddToConsole(
                                        new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Possible non plugin file installed {fileName}", ConsoleMessage.MessageLevel.WARN));
                        }

                        bool loaded = await LocalPluginUtils.DoVirtualLoading(viewModel, targetName, selectedElement, destination);
                        if (loaded)
                        {
                            viewModel.EntityViewModel.AddToConsole(
                                        new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Successfully installed locally {fileName}", ConsoleMessage.MessageLevel.SUCCESS));
                        }
                        else
                        {
                            viewModel.EntityViewModel.AddToConsole(
                                        new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Failed while installing locally {fileName}", ConsoleMessage.MessageLevel.ERROR));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Append(ex);
                        viewModel.EntityViewModel.AddToConsole(
                                    new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Failed while installing {fileName}", ConsoleMessage.MessageLevel.ERROR));
                        Console.WriteLine($"Failed to copy file {fileName} to {destination}");
                    }
                }
            }
        }

        private void OpenExplorer_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(App.ServerPath, viewModel.EntityViewModel.Entity.Name,"plugins");
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
