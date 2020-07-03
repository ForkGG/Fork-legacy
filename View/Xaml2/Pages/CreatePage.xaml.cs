using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using fork.Logic.ImportLogic;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.ViewModel;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;

namespace fork.View.Xaml2.Pages
{
    /// <summary>
    /// Interaktionslogik für CreatePage.xaml
    /// </summary>
    public partial class CreatePage : Page
    {
        private AddServerViewModel viewModel;
        private string lastPath;
        private bool isProxy = false;
        private ServerVersion.VersionType proxyType = ServerVersion.VersionType.Waterfall;
        
        public CreatePage()
        {
            InitializeComponent();
            viewModel = new AddServerViewModel();
            DataContext = viewModel;
        }
        
        private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
        {
            UnSelectProxyType();
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.VanillaServerVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
        {
            UnSelectProxyType();
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.PaperVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypeSpigot_Click(object sender, RoutedEventArgs e)
        {
            UnSelectProxyType();
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.SpigotServerVersions });
            versionComboBox.SelectedIndex = 0;
        }
        
        private void ServerTypeBungeeCord_Click(object sender, RoutedEventArgs e)
        {
            //TODO
            SelectProxyType();
            proxyType = ServerVersion.VersionType.Waterfall;
        }
        
        private void ServerTypeWaterfall_Click(object sender, RoutedEventArgs e)
        {
            SelectProxyType();
            proxyType = ServerVersion.VersionType.Waterfall;
        }

        private void SelectProxyType()
        {
            isProxy = true;
            VersionSelection.Visibility = Visibility.Collapsed;
            ConfigureSection.Visibility = Visibility.Collapsed;
            MiscSection.Visibility = Visibility.Collapsed;
            ConfigureProxySection.Visibility = Visibility.Visible;
        }

        private void UnSelectProxyType()
        {
            isProxy = false;
            VersionSelection.Visibility = Visibility.Visible;
            ConfigureSection.Visibility = Visibility.Visible;
            MiscSection.Visibility = Visibility.Visible;
            ConfigureProxySection.Visibility = Visibility.Collapsed;
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (isProxy)
            {
                string networkName = NetworkName.Text;
                if (networkName == null || networkName.Equals(""))
                {
                    networkName = "Network";
                }

                //TODO replace this with int value verifier
                int minRam, maxRam;
                if (!int.TryParse(NetworkMaxRam.Text, out maxRam))
                {
                    maxRam = 1024;
                }

                if (!int.TryParse(NetworkMinRam.Text, out minRam))
                {
                    minRam = 512;
                }
                
                JavaSettings javaSettings = new JavaSettings{MinRam = minRam, MaxRam = maxRam};
                bool createNetworkSuccess = await ServerManager.Instance.CreateNetworkAsync(networkName,proxyType, javaSettings);
            }
            else
            {   
                ServerVersion selectedVersion = (ServerVersion)versionComboBox.SelectedValue;
                //TODO check if inputs are valid / server not existing

                string serverName = ServerName.Text;
                if (serverName == null || serverName.Equals(""))
                {
                    serverName = "Server";
                }

                string worldPath = null;
                if (lastPath!=null)
                {
                    WorldValidationInfo valInfo = DirectoryValidator.ValidateWorldDirectory(new DirectoryInfo(lastPath));
                    if (valInfo.IsValid)
                    {
                        worldPath = lastPath;
                    }
                }
                bool createServerSuccess = await ServerManager.Instance.CreateServerAsync(serverName,selectedVersion, viewModel.ServerSettings, new JavaSettings(),worldPath);
            }
            viewModel.GenerateNewSettings();

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
                worldFolderPathText.Text = fbd.SelectedPath;
                lastPath = fbd.SelectedPath;
                WorldValidationInfo valInfo = DirectoryValidator.ValidateWorldDirectory(new DirectoryInfo(fbd.SelectedPath));
                if (!valInfo.IsValid)
                {
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
                }
                else
                {
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("tabSelected");
                }
            }
        }
    }
}
