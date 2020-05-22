using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using fork;
using fork.Logic.ImportLogic;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.ViewModel;
using Application = System.Windows.Application;

namespace Fork.View.Xaml2.Pages
{
    public partial class WorldsPage : Page
    {
        private ServerViewModel viewModel;
        private string lastPath;
        
        public WorldsPage(ServerViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        private void OpenExplorer_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(App.ApplicationPath,viewModel.Server.Name);
            Process.Start("explorer.exe", "-p, " + path);
        }

        private void ToggleCreateWorld_Click(object sender, RoutedEventArgs e)
        {
            switch (CreateWorldOverlay.Visibility)
            {
                case Visibility.Visible:
                    CreateWorldOverlay.Visibility = Visibility.Collapsed;
                    break;
                case Visibility.Collapsed:
                    CreateWorldOverlay.Visibility = Visibility.Visible;
                    break;
                case Visibility.Hidden:
                    CreateWorldOverlay.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void CreateWorld_Click(object sender, RoutedEventArgs e)
        {
            
        }
        
        private async void ImportWorld_Click(object sender, RoutedEventArgs e)
        {
            worldFolderPathText.Text = "Click To Select Your World";
            worldFolderPathText.Background = Brushes.Transparent;
            await ServerManager.Instance.ImportWorldAsync(viewModel, lastPath);
        }

        private void WorldDirPath_MouseDown(object sender, MouseButtonEventArgs e)
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
                    ImportWorld.IsEnabled = false;
                }
                else
                {
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("tabSelected");
                    ImportWorld.IsEnabled = true;
                }
            }
            else
            {
                ImportWorld.IsEnabled = false;
            }
        }
    }
}