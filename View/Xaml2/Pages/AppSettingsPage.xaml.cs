using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Fork;
using Fork.Logic.ImportLogic;
using Fork.Logic.Manager;
using Fork.ViewModel;
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
            string path = Path.Combine(ForkServerPath.Text);
            Process.Start("explorer.exe", "-p, " + path);
        }

        private async void ApplyNewServerDir_Click(object sender, RoutedEventArgs e)
        {
            bool result = await ServerManager.Instance.MoveEntitiesAsync(ForkServerPath.Text);
            ServerDirChangedGrid.Visibility = Visibility.Collapsed;
            serverPathBgr.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
        }

        private void ServerDirPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog {SelectedPath = ForkServerPath.Text};

            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                ForkServerPath.Text = fbd.SelectedPath;
                if (!ForkServerPath.Text.Equals(viewModel.AppSettings.ServerPath))
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

        private void BecomePatron_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.patreon.com/forkgg";
            //hack for windows only https://github.com/dotnet/corefx/issues/10361
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }
}
