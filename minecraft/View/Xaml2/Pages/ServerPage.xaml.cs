using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml2.Pages
{
    /// <summary>
    /// Interaktionslogik für ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        private ServerViewModel viewModel;
        private HashSet<Frame> subPages = new HashSet<Frame>();
        
        public ServerPage(ServerViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;

            subPages.Add(terminalPage);
            subPages.Add(settingsPage);
            subPages.Add(filesPage);
        }

        private async void ButtonStartStop_Click(object sender, RoutedEventArgs e)
        {
            StartStopButton.IsEnabled = false;
            if (viewModel.CurrentStatus == ServerStatus.STOPPED)
            {
                bool t = await ServerManager.Instance.StartServerAsync(viewModel);
                //StartStopButton.Background = (Brush) Application.Current.FindResource("buttonBgrYellow");
            }

            if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                ServerManager.Instance.StopServer(viewModel.Server);
            }
            StartStopButton.IsEnabled = true;
        }

        private void CopyIP_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AddressInfoBox.Text);
            
            new Thread(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CopyButtonText.Text = "Copied";
                    CopyButton.IsEnabled = false;
                });
                Thread.Sleep(1000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CopyButtonText.Text = "Copy";
                    CopyButton.IsEnabled = true;
                });
            }).Start();
        }
        
        private void SelectTerminal(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            terminalPage.Visibility = Visibility.Visible;
        }
        private void SelectSettings(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            settingsPage.Visibility = Visibility.Visible;
        }
        private void SelectFiles(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            filesPage.Visibility = Visibility.Visible;
        }

        private void HideAllPages()
        {
            //Save settings, if settings is closed
            if (settingsPage.Visibility == Visibility.Visible)
            {
                viewModel.UpdateSettings();
            }
            
            foreach (Frame frame in subPages)
            {
                frame.Visibility = Visibility.Hidden;
            }
        }
    }
}
