using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.logic.model.PluginModels;
using Fork.Logic.Model.ServerConsole;
using Fork.logic.Utils;
using Fork.Logic.Utils;
using Fork.View.Xaml2.Controls;
using Fork.ViewModel;
using Microsoft.Win32;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Fork.View.Xaml2.Pages.Server;

public partial class PluginsPage : Page
{
    private bool isLoadingMore;
    public PluginViewModel viewModel;

    public PluginsPage(PluginViewModel pluginViewModel)
    {
        viewModel = pluginViewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void InstallLocal_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        string downloadsDirectory = null;
        try
        {
            downloadsDirectory = Convert.ToString(
                Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"
                    , "{374DE290-123F-4565-9164-39C4925E467B}"
                    , string.Empty
                )
            );
        }
        catch (Exception)
        {
        }

        OpenFileDialog dialog = new()
        {
            Filter = "Jar file|*.jar",
            Multiselect = true
        };
        if (downloadsDirectory != null)
        {
            dialog.InitialDirectory = downloadsDirectory;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
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
                                new ConsoleMessage(
                                    $"[LOCAL PLUGIN INSTALLATION] Not installing {pluginName} because it's already installed",
                                    ConsoleMessage.MessageLevel.WARN));
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
                                    new ConsoleMessage(
                                        $"[LOCAL PLUGIN INSTALLATION] Copying {pluginDataFolder} to plugins/{pluginName}",
                                        ConsoleMessage.MessageLevel.INFO));
                                LocalPluginUtils.CopyFilesRecursively(pluginDataFolder, localData);
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.Append(ex);
                            viewModel.EntityViewModel.AddToConsole(
                                new ConsoleMessage(
                                    $"[LOCAL PLUGIN INSTALLATION] Failed to copy directory from {pluginDataFolder} to plugins/{pluginName}",
                                    ConsoleMessage.MessageLevel.ERROR));
                        }
                    }
                    else
                    {
                        viewModel.EntityViewModel.AddToConsole(
                            new ConsoleMessage(
                                $"[LOCAL PLUGIN INSTALLATION] Possible non plugin file installed {fileName}",
                                ConsoleMessage.MessageLevel.WARN));
                    }

                    bool loaded =
                        await LocalPluginUtils.DoVirtualLoading(viewModel, targetName, selectedElement, destination);
                    if (loaded)
                    {
                        viewModel.EntityViewModel.AddToConsole(
                            new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Successfully installed locally {fileName}",
                                ConsoleMessage.MessageLevel.SUCCESS));
                    }
                    else
                    {
                        viewModel.EntityViewModel.AddToConsole(
                            new ConsoleMessage(
                                $"[LOCAL PLUGIN INSTALLATION] Failed while installing locally {fileName}",
                                ConsoleMessage.MessageLevel.ERROR));
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.Append(ex);
                    viewModel.EntityViewModel.AddToConsole(
                        new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Failed while installing {fileName}",
                            ConsoleMessage.MessageLevel.ERROR));
                    Console.WriteLine($"Failed to copy file {fileName} to {destination}");
                }
            }
        }
    }

    private void OpenExplorer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        string path = Path.Combine(App.ServerPath, viewModel.EntityViewModel.Entity.Name, "plugins");
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
        double fullHeight = e.ExtentHeight;
        double currTop = e.VerticalOffset;
        double viewableHeight = e.ViewportHeight;

        if (!isLoadingMore && currTop + viewableHeight * 2 >= fullHeight)
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