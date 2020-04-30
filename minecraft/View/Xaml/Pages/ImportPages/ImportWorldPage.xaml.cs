using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using nihilus.Logic.ImportLogic;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml.Pages.ImportPages
{
    /// <summary>
    /// Interaktionslogik für ImportWorldPage.xaml
    /// </summary>
    public partial class ImportWorldPage : Page
    {
        private ImportViewModel viewModel;
        private ImportPage importPage;
        private string selectedServerPath;
        private WorldValidationInfo selectedValidationInfo;
        
        public ImportWorldPage(ImportPage importPage, ImportViewModel viewModel)
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
            ServerVersion selectedVersion = (ServerVersion)importPage.versionComboBox.SelectedValue;
            //TODO check if inputs are valid / server not existing

            bool createServerSuccess = await ServerManager.Instance.ImportWorldAsync(selectedVersion, 
                viewModel.ServerSettings, new ServerJavaSettings(), viewModel, selectedServerPath);

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
                worldFolderPathText.Text = fbd.SelectedPath;
                selectedServerPath = fbd.SelectedPath;
                WorldValidationInfo valInfo = DirectoryValidator.ValidateWorldDirectory(new DirectoryInfo(fbd.SelectedPath));
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

        private async Task PrepareImport(WorldValidationInfo valInfo, string serverPath)
        {
            if (!valInfo.IsValid)
            {
                serverInfoPanel.Visibility = Visibility.Hidden;
                worldFolderPathBorder.BorderBrush = Brushes.Red;
                createButton.IsEnabled = false;
            }
            else
            {
                createButton.IsEnabled = true;
                worldFolderPathBorder.BorderBrush = (Brush)FindResource("backgroundLight");
                int start = serverPath.LastIndexOf(Path.DirectorySeparatorChar)+1;
                int end = serverPath.Length - start;
                viewModel.ServerSettings = new ServerSettings(serverPath.Substring(start, end));
                CheckServerName();
                serverInfoPanel.Visibility = Visibility.Visible;
            }
        }

        private void serverName_TextChanged(object sender, EventArgs e)
        {
            if (!worldFolderPathText.Text.Equals(""))
            {
                CheckServerName();
            }
        }
    }
}
