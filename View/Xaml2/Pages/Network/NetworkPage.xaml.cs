using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Network
{
    public partial class NetworkPage : Page
    {
        private NetworkViewModel viewModel;
        private HashSet<Frame> subPages = new HashSet<Frame>();
        
        public NetworkPage(NetworkViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
            
            subPages.Add(terminalPage);
            subPages.Add(settingsPage);
            subPages.Add(pluginsPage);
        }
        
        public void OpenTerminal()
        {
            TerminalTab.IsChecked = true;
            SelectTerminal(this, new RoutedEventArgs());
        }

        private async void ButtonStartStop_Click(object sender, RoutedEventArgs e)
        {
            StartStopButton.IsEnabled = false;
            TerminalTab.IsChecked = true;
            SelectTerminal(this,e);
            if (viewModel.CurrentStatus == ServerStatus.STOPPED)
            {
                await ServerManager.Instance.StartNetworkAsync(viewModel, viewModel.Network.SyncServers);
            }

            else if (viewModel.CurrentStatus == ServerStatus.STARTING)
            {
                ServerManager.Instance.KillEntity(viewModel);
            }

            else if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                await ServerManager.Instance.StopNetworkAsync(viewModel, viewModel.Network.SyncServers);
            }
            StartStopButton.IsEnabled = true;
        }

        private void CopyIP_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AddressInfoBox.Text);
            
            new Thread(() =>
            {
                Application.Current.Dispatcher?.Invoke(() =>
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
        
        private void SelectPlugins(object sender, RoutedEventArgs e)
        {
            HideAllPages();
            pluginsPage.Visibility = Visibility.Visible;
        }

        private void HideAllPages()
        {
            //Save settings, if settings is closed
            if (settingsPage.Visibility == Visibility.Visible)
            {
                viewModel.SaveSettings();
            }
            
            foreach (Frame frame in subPages)
            {
                frame.Visibility = Visibility.Hidden;
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            TerminalTab.IsChecked = true;
            SelectTerminal(this, e);
            ServerManager.Instance.RestartNetworkAsync(viewModel, viewModel.Network.SyncServers);
        }
    }
}
