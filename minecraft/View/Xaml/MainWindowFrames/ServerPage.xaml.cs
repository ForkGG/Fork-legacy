using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                ServerManager.Instance.StopServer(viewModel.Server);
            }
            else if (viewModel.CurrentStatus == ServerStatus.STOPPED)
            {
                ServerManager.Instance.StartServer(viewModel);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(viewModel);
            settingsWindow.ShowDialog();
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
            AddPlayerInput.Visibility = Visibility.Visible;
        }

        private void AddPlayerAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.PlayerExists(AddPlayerInputTextBox.Text))
            {
                viewModel.AddPlayerToWhiteList(AddPlayerInputTextBox.Text);
            }
            else
            {
                //TODO
            }
            
            AddPlayerCloseButton_Click(sender,e);
        }

        private void AddPlayerCloseButton_Click(object sender, RoutedEventArgs e)
        {
            AddPlayerInput.Visibility = Visibility.Collapsed;
            AddPlayerInputTextBox.Text = "";
        }

        private void AddPlayer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AddPlayerCloseButton_Click(sender,e);
        }
    }
}
