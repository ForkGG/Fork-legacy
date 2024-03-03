using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Settings;

public partial class SettingsPage : Page
{
    private SettingsViewModel viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = viewModel;
    }

    private void FileSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ISettingsPage settingsPage)
        {
            UpdateSettingsFrame(settingsPage);
        }
    }

    public void UpdateSettingsFrame(ISettingsPage settingsPage)
    {
        if (settingsPage == null)
        {
            return;
        }

        settingsFrame.Content = settingsPage;
    }

    private void OpenExplorer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        string path = Path.Combine(App.ServerPath, viewModel.EntityViewModel.Name);
        Process.Start("explorer.exe", "-p, " + path);
    }
}