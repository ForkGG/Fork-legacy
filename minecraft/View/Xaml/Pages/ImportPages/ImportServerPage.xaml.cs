using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using nihilus.Logic.ImportLogic;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml.Pages.ImportPages
{
    /// <summary>
    /// Interaktionslogik für ImportServerPage.xaml
    /// </summary>
    public partial class ImportServerPage : Page
    {
        private ImportViewModel viewModel;
        private ImportPage importPage;
        private Server importableServer;
        private string selectedServerPath;
        private ServerValidationInfo selectedValidationInfo;
        
        public ImportServerPage(ImportPage importPage, ImportViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = viewModel;
            this.importPage = importPage;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RaiseImportPreviousEvent(sender, e);
        }

        private async void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            bool createServerSuccess = await ServerManager.Instance.ImportServerAsync(importableServer, 
                viewModel, selectedValidationInfo, selectedServerPath, serverName.Text);

            //TODO Do something if creating fails

            // Close the page
            viewModel.RaiseImportCloseEvent(sender, e);
            Overlay.Visibility = Visibility.Hidden;
            viewModel.RaiseImportPreviousEvent(sender, e);
        }

        private async void BtnSelectServer_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (selectedServerPath!=null)
            {
                fbd.SelectedPath = selectedServerPath;
            }
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                serverFolderPathText.Text = fbd.SelectedPath;
                selectedServerPath = fbd.SelectedPath;
                ServerValidationInfo valInfo = DirectoryValidator.ValidateServerDirectory(new DirectoryInfo(fbd.SelectedPath));
                selectedValidationInfo = valInfo;
                await PrepareImport(valInfo, fbd.SelectedPath);
            }
        }

        private void CheckServerName()
        {
            string serverNameString = serverName.Text;
            foreach (ServerViewModel viewModel in ApplicationManager.Instance.MainViewModel.Servers)
            {
                if (viewModel.Server == null)
                {
                    continue;
                }
                if (viewModel.Server.Name.Equals(serverNameString))
                {
                    serverName.BorderBrush = Brushes.Red;
                    createButton.IsEnabled = false;
                    return;
                }
            }
            serverName.BorderBrush = (Brush)FindResource("backgroundLight");
            createButton.IsEnabled = true;
        }

        private async Task PrepareImport(ServerValidationInfo valInfo, string serverPath)
        {
            if (!valInfo.IsValid)
            {
                serverInfoPanel.Visibility = Visibility.Hidden;
                serverFolderPathBorder.BorderBrush = Brushes.Red;
                createButton.IsEnabled = false;
                UnCheckImportBlock(importData);
            }
            else
            {
                CheckImportBlock(importData);
                createButton.IsEnabled = true;
                serverFolderPathBorder.BorderBrush = (Brush)FindResource("backgroundLight");
                int start = serverPath.LastIndexOf(Path.DirectorySeparatorChar)+1;
                int end = serverPath.Length - start;
                importableServer = await ServerManager.Instance.PrepareServerImportAsync(serverPath,
                    serverPath.Substring(start, end), (ServerVersion) importPage.versionComboBox.SelectedItem);
                viewModel.ServerSettings = importableServer.ServerSettings;
                CheckServerName();
                serverInfoPanel.Visibility = Visibility.Visible;
            }
            if (valInfo.ServerProperties)
            {
                CheckImportBlock(importSettings);
            } else
            {
                UnCheckImportBlock(importSettings);
            }
            if (valInfo.WorldValidationInfo.Data)
            {
                CheckImportBlock(importPlayerData);
            } else
            {
                UnCheckImportBlock(importPlayerData);
            }
            if (valInfo.Whitelist)
            {
                CheckImportBlock(importWhitelist);
            } else
            {
                UnCheckImportBlock(importWhitelist);
            }
            if (valInfo.Ops)
            {
                CheckImportBlock(importOPlist);
            } else
            {
                UnCheckImportBlock(importOPlist);
            }
            if (valInfo.Banlist)
            {
                CheckImportBlock(importBanlist);
            } else
            {
                UnCheckImportBlock(importBanlist);
            }
        }

        private void CheckImportBlock(TextBlock textBlock)
        {
            textBlock.Foreground = Brushes.Green;
            textBlock.Text = "\xE876";
        }
        private void UnCheckImportBlock(TextBlock textBlock)
        {
            textBlock.Foreground = Brushes.Red;
            textBlock.Text = "\xE5CD";
        }

        private void serverName_TextChanged(object sender, EventArgs e)
        {
            if (!serverFolderPathText.Text.Equals(""))
            {
                CheckServerName();
            }
        }
    }
}
