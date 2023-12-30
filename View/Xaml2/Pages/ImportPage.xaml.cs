using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Fork.logic.Utils;
using Fork.Logic.ImportLogic;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.ServerConsole;
using Fork.Logic.Utils;
using Fork.View.Xaml2.Pages.Server;
using Fork.ViewModel;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;

namespace Fork.View.Xaml2.Pages
{
    /// <summary>
    /// Interaktionslogik für ImportServerPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        private ImportViewModel viewModel;
        private string lastPath;
        
        public ImportPage()
        {
            InitializeComponent();
            viewModel = new ImportViewModel();
            DataContext = viewModel;
        }
        
        private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.VanillaServerVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypeSnapshot_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.SnapshotServerVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypeSpigot_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.SpigotVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.PaperVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypePurpur_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.PurpurVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypeFabric_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.FabricServerVersions });
            versionComboBox.SelectedIndex = 0;
        }
        
        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            ServerVersion selectedVersion = (ServerVersion)versionComboBox.SelectedValue;
            //TODO check if inputs are valid / server not existing

            if (lastPath == null )
            {
                throw new Exception("Import Button should not be clickable, as path is not valid");
            }
            
            DirectoryInfo oldDir = new DirectoryInfo(lastPath);
            string serverName = oldDir.Name;
            ServerValidationInfo validationInfo = DirectoryValidator.ValidateServerDirectory(oldDir);

            bool createServerSuccess = await ServerManager.Instance.ImportServerAsync(selectedVersion, validationInfo, oldDir.FullName, serverName);
            ServerViewModel viewModel = ApplicationManager.Instance.MainViewModel.SelectedEntity as ServerViewModel;

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += async(e, a) =>
            {
                string serverPath = Path.Combine(App.ServerPath, serverName);
                string pluginsFolder = Path.Combine(serverPath, "plugins");

                if (Directory.Exists(pluginsFolder))
                {
                    viewModel.AddToConsole(
                                new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Loading plugins", ConsoleMessage.MessageLevel.INFO));

                    string[] files = Directory.GetFiles(pluginsFolder);
                    List<string> jarFiles = files
                        .Where(archivo => Path.GetExtension(archivo).Equals(".jar", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    PluginsPage pluginsPage = viewModel.PluginsPage as PluginsPage;

                    Application.Current.Dispatcher.Invoke(async() =>
                    {
                        foreach (string file in jarFiles)
                        {
                            string pluginName = LocalPluginUtils.ReadPluginName(file);
                            string fileName = Path.GetFileName(file);
                            string targetName = pluginName ?? StringUtils.BeautifyPluginName(fileName[..^4]);

                            if (pluginName == null)
                            {
                                viewModel.AddToConsole(
                                    new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Possible non plugin file installed {fileName}", ConsoleMessage.MessageLevel.WARN));
                            }

                            bool added = await LocalPluginUtils.DoVirtualLoading(pluginsPage.viewModel, targetName, file, file);
                            if (added)
                            {
                                viewModel.AddToConsole(
                                    new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Loaded plugin from imported server {targetName}",
                                    ConsoleMessage.MessageLevel.SUCCESS));
                            }
                            else
                            {
                                viewModel.AddToConsole(
                                    new ConsoleMessage($"[LOCAL PLUGIN INSTALLATION] Failed to load plugin from imported server {targetName}",
                                    ConsoleMessage.MessageLevel.ERROR));
                            }
                        }
                    });
                }
            };
            timer.AutoReset = false;
            timer.Interval = 1000;
            timer.Start();
            /*
             * So, apparently, if we try to load the plugins right
             * after the server has been imported, it messes up with
             * server directory watchdog and crashes the application.
             * This is the approach I've ended up with, it's not the
             * best, and I should probably relay on some other kind of
             * event, but for now it works
             * - KarmaDev
             */

            //TODO Do something if creating fails
        }

        private void ServerDirPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (lastPath != null)
            {
                fbd.SelectedPath = lastPath;
            }
            
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                serverFolderPathText.Text = fbd.SelectedPath;
                lastPath = fbd.SelectedPath;
                ServerValidationInfo valInfo = null;
                try
                {
                    valInfo = DirectoryValidator.ValidateServerDirectory(new DirectoryInfo(fbd.SelectedPath));
                }
                catch(Exception ex)
                {
                    ErrorLogger.Append(ex);
                }
                if (valInfo == null || !valInfo.IsValid)
                {
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
                    ImportConfirmButton.IsEnabled = false;
                }
                else
                {
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("tabSelected");
                    ImportConfirmButton.IsEnabled = true;
                }
            }
        }
    }
}
