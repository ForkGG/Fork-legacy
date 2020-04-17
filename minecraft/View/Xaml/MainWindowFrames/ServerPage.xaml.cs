using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml.MainWindowFrames
{
    /// <summary>
    /// Interaction logic for ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        private ServerViewModel viewModel;

        public ServerPage(ServerViewModel viewModel)
        {
            this.viewModel = viewModel;
            DataContext = viewModel;
            
            InitializeComponent();
            viewModel.SettingsPage.CloseSettingsEvent += HandleCloseSettingsEvent;
        }

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentStatus == ServerStatus.STOPPED)
            {
                await ServerManager.Instance.StartServerAsync(viewModel).ConfigureAwait(true);
            }else if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                ServerManager.Instance.StopServer(viewModel.Server);
            }
            
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Visible;
        }

        #region autoscrolling
        /// <summary>
        /// Automatically scrolls the scrollviewer
        /// </summary>

        private bool AutoScroll = true;

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ExtentHeight);
            }
        }
        #endregion autoscrolling

        private void AddToWhiteList_Click(object sender, RoutedEventArgs e)
        {
            WhitelistPlayerInput.Visibility = Visibility.Visible;
        }

        private void WhitelistPlayerConfirm_Click(object sender, RoutedEventArgs e)
        {
            PlayerManager.Instance.WhitelistPlayer(viewModel,WhitelistPlayerName.Text);
            
            WhitelistPlayerAbort_Click(sender,e);
        }

        private void WhitelistPlayerAbort_Click(object sender, RoutedEventArgs e)
        {
            WhitelistPlayerInput.Visibility = Visibility.Collapsed;
            WhitelistPlayerName.Text = "";
        }

        private void CopyIP_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AddressInfoBox.Text);
            
            new Thread(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CopyButton.Content = "Copied";
                    CopyButton.IsEnabled = false;
                });
                Thread.Sleep(1000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CopyButton.Content = "Copy";
                    CopyButton.IsEnabled = true;
                });
            }).Start();
        }

        private void WhitelistPlayer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            WhitelistPlayerAbort_Click(sender,e);
        }

        private void Settings_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseSettings();
        }

        private void Player_Ban(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string name = item.CommandParameter as string;
            PlayerManager.Instance.BanPlayer(viewModel,name);
        }
        private void Player_OP(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string name = item.CommandParameter as string;
            PlayerManager.Instance.OpPlayer(viewModel,name);
        }
        private void Player_Kick(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string name = item.CommandParameter as string;
            PlayerManager.Instance.KickPlayer(viewModel,name);
        }
        private void Player_Deop(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string name = item.CommandParameter as string;
            PlayerManager.Instance.DeopPlayer(viewModel,name);
        }
        private void Player_Unban(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string name = item.CommandParameter as string;
            PlayerManager.Instance.UnBanPlayer(viewModel,name);
        }
        
        private void Player_Unwhitelist(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string name = item.CommandParameter as string;
            PlayerManager.Instance.UnWhitelistPlayer(viewModel,name);
        }

        private void HandleCloseSettingsEvent(object sender, EventArgs e)
        {
            CloseSettings();
        }

        private void CloseSettings()
        {
            SettingsGrid.Visibility = Visibility.Hidden;
            viewModel.UpdateSettings();
        }
    }
}
