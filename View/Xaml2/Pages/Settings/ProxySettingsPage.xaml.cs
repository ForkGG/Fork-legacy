using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fork.Logic.Model.ProxyModels;
using Fork.Logic.Model.Settings;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Settings;

/// <summary>
///     Interaktionslogik für ProxySettingsPage.xaml
/// </summary>
public partial class ProxySettingsPage : Page, ISettingsPage
{
    private readonly NetworkViewModel networkViewModel;

    public ProxySettingsPage(SettingsViewModel viewModel, SettingsFile settingsFile)
    {
        InitializeComponent();
        SettingsFile = settingsFile;
        networkViewModel = viewModel.EntityViewModel as NetworkViewModel;
        DataContext = networkViewModel;
    }

    public SettingsFile SettingsFile { get; set; }
    public string FileName => Path.GetFileNameWithoutExtension(SettingsFile.FileInfo.FullName);
    public string FileExtension => Path.GetExtension(SettingsFile.FileInfo.FullName);

    public async Task SaveSettings()
    {
        networkViewModel.SaveConfig();
    }

    public void Reload()
    {
        object oldDataContext = DataContext;
        DataContext = null;
        DataContext = oldDataContext;
    }

    private void AddExternalServer_Click(object sender, MouseButtonEventArgs e)
    {
        AddOverlay.Visibility = Visibility.Visible;
    }

    private void AddGroup_Click(object sender, RoutedEventArgs e)
    {
        AddGroupOverlay.Visibility = Visibility.Visible;
    }

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        AddUserOverlay.Visibility = Visibility.Visible;
    }

    private void AddServerCancel_Click(object sender, RoutedEventArgs e)
    {
        AddOverlay.Visibility = Visibility.Collapsed;
        newServerAddress.Clear();
        newServerName.Clear();
        newServerMotd.Clear();
    }

    private void AddServerApply_Click(object sender, RoutedEventArgs e)
    {
        string name = newServerName.Text;
        Logic.Model.Settings.Server server = new()
        {
            address = newServerAddress.Text,
            motd = newServerMotd.Text,
            ForkServer = false,
            restricted = false
        };
        networkViewModel.AddServer(server, name);
        AddServerCancel_Click(sender, e);
    }

    private void AddGroupCancel_Click(object sender, RoutedEventArgs e)
    {
        AddGroupOverlay.Visibility = Visibility.Collapsed;
        newGroupName.Clear();
    }

    private void AddGroupApply_Click(object sender, RoutedEventArgs e)
    {
        string name = newGroupName.Text;
        Permission permission = new(name, new ObservableCollection<string>());
        networkViewModel.AddPermission(permission);
        AddGroupCancel_Click(sender, e);
    }

    private void AddUserCancel_Click(object sender, RoutedEventArgs e)
    {
        AddUserOverlay.Visibility = Visibility.Collapsed;
        newUserName.Clear();
    }

    private void AddUserApply_Click(object sender, RoutedEventArgs e)
    {
        string name = newUserName.Text;
        Group group = new(name, new ObservableCollection<string>());
        networkViewModel.AddGroup(group);
        AddUserCancel_Click(sender, e);
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
    }
}