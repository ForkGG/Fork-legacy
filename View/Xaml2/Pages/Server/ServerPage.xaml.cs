using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Fork.Logic.Controller;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Server;

/// <summary>
///     Interaktionslogik für ServerPage.xaml
/// </summary>
public partial class ServerPage : Page
{
    private readonly HashSet<Frame> subPages = new();
    private readonly ServerViewModel viewModel;

    public ServerPage(ServerViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = this.viewModel;

        subPages.Add(terminalPage);
        subPages.Add(settingsPage);
        subPages.Add(worldsPage);
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
        SelectTerminal(this, e);
        if (viewModel.CurrentStatus == ServerStatus.STOPPED)
        {
            await ServerManager.Instance.StartServerAsync(viewModel);
        }

        else if (viewModel.CurrentStatus == ServerStatus.STARTING)
        {
            await Task.Run(() => ServerManager.Instance.KillEntity(viewModel));
        }

        else if (viewModel.CurrentStatus == ServerStatus.RUNNING)
        {
            await Task.Run(() => ServerManager.Instance.StopServer(viewModel));
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
                CopyIpPopup.IsOpen = true;
            });
            Thread.Sleep(1000);
            Application.Current.Dispatcher.Invoke(() =>
            {
                CopyIpPopup.IsOpen = false;
            });
        }) { IsBackground = true }.Start();
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

    private void SelectWorlds(object sender, RoutedEventArgs e)
    {
        HideAllPages();
        worldsPage.Visibility = Visibility.Visible;
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

        foreach (Frame frame in subPages) frame.Visibility = Visibility.Hidden;
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        TerminalTab.IsChecked = true;
        SelectTerminal(this, e);
        ServerManager.Instance.RestartServerAsync(viewModel);
    }

    private async void AvailabilityButton_OnClick(object sender, RoutedEventArgs e)
    {
        viewModel.LastAvailabilityCheckResult = EntityViewModel.AvailabilityCheckResult.PENDING;
        bool isAvailable = await Task.Run(() => new APIController().IsServerReachable(viewModel.AddressInfo));
        viewModel.LastAvailabilityCheckResult = isAvailable ? EntityViewModel.AvailabilityCheckResult.OK : EntityViewModel.AvailabilityCheckResult.FAILED;
    }
}