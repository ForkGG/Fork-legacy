using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.ViewModel;

namespace fork.View.Xaml2.Pages
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
            {   // Content unchanged : uSelectPlayerList
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
            //TODO
        }

        private void Player_Unban(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            PlayerManager.Instance.UnBanPlayer(viewModel, item?.CommandParameter as string);
        }
    }
}
