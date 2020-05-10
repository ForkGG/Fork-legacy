using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        
        public CreatePage()
        {
            InitializeComponent();
            viewModel = new AddServerViewModel();
            DataContext = viewModel;
        }
        
        private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.VanillaServerVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.PaperVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypeSpigot_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.SpigotServerVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
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

            bool createServerSuccess = await ServerManager.Instance.CreateServerAsync(serverName,selectedVersion, viewModel.ServerSettings, new ServerJavaSettings(),worldPath);

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
