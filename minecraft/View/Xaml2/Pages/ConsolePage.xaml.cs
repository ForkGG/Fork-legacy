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
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
using nihilus.ViewModel;

namespace nihilus.View.Xaml2.Pages
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


        private async void ButtonStartStop_Click(object sender, RoutedEventArgs e)
        {
            StartStopButton.IsEnabled = false;
            if (viewModel.CurrentStatus == ServerStatus.STOPPED)
            {
                bool t = await ServerManager.Instance.StartServerAsync(viewModel);
                if (!t)
                {
                    //TODO display error
                    StartStopButton.IsEnabled = true;
                    return;
                }
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
    }
}
