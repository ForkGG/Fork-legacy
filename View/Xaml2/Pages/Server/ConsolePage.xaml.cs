using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using fork.Logic.Manager;
using fork.ViewModel;

namespace fork.View.Xaml2.Pages.Server
{
    /// <summary>
    /// Interaktionslogik für ConsolePage.xaml
    /// </summary>
    public partial class ConsolePage : Page
    {
        private ServerViewModel viewModel;
        
        public ConsolePage(ServerViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;

            PlayerToWhitelist.KeyDown += HandleKeyDownText;
        }
        

        
        #region autoscrolling
        /// <summary>
        /// Automatically scrolls the scrollviewer
        /// </summary>

        private bool AutoScroll = true;

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : uSelectPlayerList
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
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
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
        }
        #endregion autoscrolling

        private void Player_Ban(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            PlayerManager.Instance.BanPlayer(viewModel, item?.CommandParameter as string);
        }
        
        private void Player_OP(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            PlayerManager.Instance.OpPlayer(viewModel, item?.CommandParameter as string);
        }
        private void Player_Kick(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            PlayerManager.Instance.KickPlayer(viewModel, item?.CommandParameter as string);
        }
        
        private void Player_Deop(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            PlayerManager.Instance.DeopPlayer(viewModel, item?.CommandParameter as string);
        }

        private void Player_Unwhitelist(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            PlayerManager.Instance.UnWhitelistPlayer(viewModel, item?.CommandParameter as string);
        }

        private void AddToWhiteList_Click(object sender, RoutedEventArgs e)
        {
            AddWhitelistPanel.Visibility = Visibility.Visible;
            PlayerToWhitelist.Focus();
        }
        
        private void WhitelistAddConfirm_Click(object sender, RoutedEventArgs e)
        {
            AddWhitelistPanel.Visibility = Visibility.Collapsed;
            PlayerManager.Instance.WhitelistPlayer(viewModel,PlayerToWhitelist.Text);
            PlayerToWhitelist.Text = "";
        }

        private void Player_Unban(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            PlayerManager.Instance.UnBanPlayer(viewModel, item?.CommandParameter as string);
        }

        private void HandleKeyDownText(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                WhitelistAddConfirm_Click(sender, e);
            }
        }
    }
}
