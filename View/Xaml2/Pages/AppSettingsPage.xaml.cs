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
using fork.ViewModel;
using Application = System.Windows.Application;

namespace Fork.View.Xaml2.Pages
{
    public partial class AppSettingsPage : Page
    {
        private AppSettingsViewModel viewModel;
        
        public AppSettingsPage(AppSettingsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        private void CloseAppSettings()
        {
            viewModel.ClosePage(this);
        }

        private void OpenForkServerDir_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(forkServerPath.Text);
            Process.Start("explorer.exe", "-p, " + path);
        }

        private void ServerDirPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog {SelectedPath = forkServerPath.Text};

            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                forkServerPath.Text = fbd.SelectedPath;
                if (!forkServerPath.Text.Equals(viewModel.AppSettings.ServerPath))
                {
                    ServerDirChangedGrid.Visibility = Visibility.Visible;
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("tabSelected");
                }
                else
                {
                    ServerDirChangedGrid.Visibility = Visibility.Collapsed;
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
                }
                
            }
        }
    }
}
