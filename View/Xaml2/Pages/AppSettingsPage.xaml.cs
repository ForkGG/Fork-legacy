using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fork;
using Fork.Logic.ImportLogic;
using Fork.Logic.Logging;
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

        private void OpenForkServerDir_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(ForkServerPath.Text);
            Process.Start("explorer.exe", "-p, " + path);
        }

        private async void ApplyNewServerDir_Click(object sender, RoutedEventArgs e)
        {
            bool result;
            try
            {
                result = await ServerManager.Instance.MoveEntitiesAsync(ForkServerPath.Text);
            }
            catch (Exception ex)
            {
                ServerDirChangeErrorGrid.Visibility = Visibility.Visible;
                if (ex is UnauthorizedAccessException)
                {
                    ErrorMsgBox.Text = "Fork can't access \"" + ForkServerPath.Text +
                                       "\"! Please try to use another directory.";
                }
                else
                {
                    ErrorMsgBox.Text = ex.Message;
                }
                ErrorLogger.Append(ex);
                return;
            }
            if (!result)
            {
                ServerDirChangeErrorGrid.Visibility = Visibility.Visible;
                ErrorMsgBox.Text =
                    "Unknown error, this should not happen, please report to a Fork developer. Sadly this might have broken the functionality of Fork.";
                return;
            }
            ServerDirChangeErrorGrid.Visibility = Visibility.Collapsed;
            ServerDirChangedGrid.Visibility = Visibility.Collapsed;
            ResetServerDirButton.Visibility = Visibility.Collapsed;
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
                    ResetServerDirButton.Visibility = Visibility.Visible;
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("tabSelected");
                }
                else
                {
                    ServerDirChangedGrid.Visibility = Visibility.Collapsed;
                    ResetServerDirButton.Visibility = Visibility.Collapsed;
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("textBackground");
                }
                
            }
        }
        
        private void JavaPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog{Multiselect = false, Filter = "Java executable|java.exe", Title = "Select a java.exe"};
            if (new DirectoryInfo(ForkDefaultJavaPath.Text.Replace(@"\java.exe","")).Exists)
            {
                ofd.InitialDirectory = ForkDefaultJavaPath.Text.Replace(@"\java.exe","");
            }
            else
            {
                ofd.InitialDirectory = @"C:\Program Files";
            }
                
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                ForkDefaultJavaPath.Text = ofd.FileName;
            }
        }
        
        private void DefaultJavaDirReset_Click(object sender, RoutedEventArgs e)
        {
            ForkDefaultJavaPath.Text = "java.exe";
        }
        
        private void ResetServerDir_Click(object sender, RoutedEventArgs e)
        {
            ForkServerPath.Text = viewModel.AppSettings.ServerPath;
            ServerDirChangeErrorGrid.Visibility = Visibility.Collapsed;
            ServerDirChangedGrid.Visibility = Visibility.Collapsed;
            ResetServerDirButton.Visibility = Visibility.Collapsed;
            serverPathBgr.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
        }

        private void BecomePatron_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.patreon.com/forkgg";
            //hack for windows only https://github.com/dotnet/corefx/issues/10361
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private async void UseBetaChanged(object sender, RoutedEventArgs e)
        {
            UseBetaBox.IsEnabled = false;
            if (viewModel.MainViewModel.IsBetaVersion != viewModel.AppSettings.UseBetaVersions)
            {
                restartNotice.Visibility = Visibility.Visible;
            }
            else
            {
                restartNotice.Visibility = Visibility.Collapsed;
            }
            viewModel.MainViewModel.CheckForkVersion();
            await viewModel.WriteAppSettingsAsync();
            UseBetaBox.IsEnabled = true;
        }
    }
}
