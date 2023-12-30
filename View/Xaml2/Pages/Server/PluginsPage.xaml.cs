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
                    string pluginName = ReadPluginName(selectedElement);
                    string fileName = Path.GetFileName(selectedElement);
                    string targetName = pluginName ?? StringUtils.BeautifyPluginName(fileName[..^4]);

                    string destination = Path.Combine(path, $"{targetName}.jar");

                    try
                    {
                        File.Copy(selectedElement, destination, true);
                        
                        if (pluginName != null)
                        {
                            bool install = true;
                            foreach (InstalledPlugin iPlugin in viewModel.InstalledPlugins)
                            {
                                string iName;
                                if (iPlugin.Plugin == null)
                                {
                                    iName = iPlugin.Name;
                                } else
                                {
                                    iName = iPlugin.Plugin.name;
                                }

                                if (iName.Equals(pluginName))
                                {
                                    install = false;
                                    break;
                                }
                            }

                            if (!install)
                            {
                                Console.WriteLine($"Denied to install {pluginName} because it's already installed");
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
                                    Console.WriteLine($"Copying {pluginDataFolder} to {localData}");
                                    CopyFilesRecursively(pluginDataFolder, localData);
                                }
                            } catch (Exception ex)
                            {
                                ErrorLogger.Append(ex);
                                Console.WriteLine($"Failed to copy directory from {pluginDataFolder} to {localData}");
                            }

                            InstalledPlugin ip = new InstalledPlugin
                            {
                                LocalId = new Random().Next() * new Random().Next(),
                                Name = targetName,
                                IsSpigetPlugin = false,
                                IsDownloaded = true,
                                LocalPlugin = new Fork.Logic.Model.PluginModels.File {
                                    type = "jar",
                                    actualType = "jar",
                                    size = new FileInfo(selectedElement).Length,
                                    sizeUnit = "bytes",
                                    url = destination
                                }
                            };

                            viewModel.InstalledPlugins.Add(ip);
                            viewModel.EnablePlugin(ip);
                            await PluginManager.Instance.EnablePluginAsync(ip, viewModel);
                        } else
                        {
                            Console.WriteLine($"Possible non plugin file {destination} was moved to plugins folder");
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.Append(ex);
                        Console.WriteLine($"Failed to copy file {fileName} to {destination}");
                    }
                }
            }
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private string ReadPluginName(string file)
        {
            try
            {
                using (ZipFile zip = new ZipFile(file))
                {
                    ZipEntry entry = zip.GetEntry("bungee.yml");
                    if (entry == null)
                    {
                        entry = zip.GetEntry("paper-plugin.yml");
                        if (entry == null)
                        {
                            entry = zip.GetEntry("plugin.yml");
                        }
                    }

                    if (entry != null)
                    {
                        using (Stream stream = zip.GetInputStream(entry))
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string rawYaml = sr.ReadToEnd();
                            dynamic yaml = new DeserializerBuilder().Build().Deserialize(new StringReader(rawYaml));

                            return yaml["name"];
                        }
                    }
                }
            } 
            catch (Exception ex)
            {
                ErrorLogger.Append(ex);
                Console.WriteLine($"Failed to read {file} contents");
            } 

            return null;
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
