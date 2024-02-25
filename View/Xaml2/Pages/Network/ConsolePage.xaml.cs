using System.Windows.Controls;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Network;

public partial class ConsolePage : Page
{
    private readonly NetworkViewModel viewModel;

    public ConsolePage(NetworkViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = this.viewModel;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string query = SearchBox.Text;
        viewModel.ApplySearchQueryToConsole(query);
    }


    #region autoscrolling

    /// <summary>
    ///     Automatically scrolls the scrollviewer
    /// </summary>
    private bool AutoScroll = true;

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        ScrollViewer scrollViewer = sender as ScrollViewer;
        // User scroll event : set or unset auto-scroll mode
        if (e.ExtentHeightChange == 0)
        {
            // Content unchanged : uSelectPlayerList
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                // Scroll bar is in bottom
                // Set auto-scroll mode
                AutoScroll = true;
            }
            else
            {
                // Scroll bar isn't in bottom
                // Unset auto-scroll mode
                AutoScroll = false;
            }
        }

        // Content scroll event : auto-scroll eventually
        if (AutoScroll && e.ExtentHeightChange != 0)
        {
            // Content changed and auto-scroll mode set
            // Autoscroll
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
        }
    }

    #endregion autoscrolling
}