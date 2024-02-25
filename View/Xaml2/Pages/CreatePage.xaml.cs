using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Fork.Logic.ImportLogic;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.ViewModel;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;

namespace Fork.View.Xaml2.Pages;

public partial class CreatePage : Page
{
    private bool isProxy;
    private string lastPath;
    private ServerVersion.VersionType proxyType = ServerVersion.VersionType.Waterfall;
    private readonly AddServerViewModel viewModel;

    public CreatePage()
    {
        InitializeComponent();
        viewModel = new AddServerViewModel();
        DataContext = viewModel;
    }

    private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
    {
        UnSelectProxyType();
        versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty,
            new Binding { Source = viewModel.VanillaServerVersions });
        versionComboBox.SelectedIndex = 0;
    }

    private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
    {
        UnSelectProxyType();
        versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = viewModel.PaperVersions });
        versionComboBox.SelectedIndex = 0;
    }

    private void ServerTypeSpigot_Click(object sender, RoutedEventArgs e)
    {
        UnSelectProxyType();
        versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty,
            new Binding { Source = viewModel.SpigotServerVersions });
        versionComboBox.SelectedIndex = 0;
    }

    private void ServerTypeWaterfall_Click(object sender, RoutedEventArgs e)
    {
        SelectProxyType();
        proxyType = ServerVersion.VersionType.Waterfall;
    }

    private void ServerTypeBungeeCord_Click(object sender, RoutedEventArgs e)
    {
        SelectProxyType();
        proxyType = ServerVersion.VersionType.BungeeCord;
    }

    private void ServerTypeSnapshot_Click(object sender, RoutedEventArgs e)
    {
        UnSelectProxyType();
        versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty,
            new Binding { Source = viewModel.SnapshotServerVersions });
        versionComboBox.SelectedIndex = 0;
    }

    private void ServerTypePurpur_Click(object sender, RoutedEventArgs e)
    {
        UnSelectProxyType();
        versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = viewModel.PurpurVersions });
        versionComboBox.SelectedIndex = 0;
    }

    private void ServerTypeFabric_Click(object sender, RoutedEventArgs e)
    {
        UnSelectProxyType();
        versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty,
            new Binding { Source = viewModel.FabricServerVersions });
        versionComboBox.SelectedIndex = 0;
    }

    private void SelectProxyType()
    {
        isProxy = true;
        VersionSelection.Visibility = Visibility.Collapsed;
        ConfigureSection.Visibility = Visibility.Collapsed;
        MiscSection.Visibility = Visibility.Collapsed;
        ConfigureProxySection.Visibility = Visibility.Visible;
    }

    private void UnSelectProxyType()
    {
        isProxy = false;
        VersionSelection.Visibility = Visibility.Visible;
        ConfigureSection.Visibility = Visibility.Visible;
        MiscSection.Visibility = Visibility.Visible;
        ConfigureProxySection.Visibility = Visibility.Collapsed;
    }

    private async void BtnApply_Click(object sender, RoutedEventArgs e)
    {
        CreateBtn.IsEnabled = false;
        char[] illegalDirChars = Path.GetInvalidFileNameChars();
        if (isProxy)
        {
            string networkName = NetworkName.Text;
            string refinedNetworkName = networkName;
            foreach (char c in networkName)
                if (illegalDirChars.Contains(c))
                {
                    refinedNetworkName = refinedNetworkName.Replace(c + "", "");
                }

            if (refinedNetworkName.Equals(""))
            {
                refinedNetworkName = "Network";
            }

            //TODO replace this with int value verifier
            int maxRam;
            if (!int.TryParse(NetworkMaxRam.Text, out maxRam))
            {
                maxRam = 1024;
            }

            JavaSettings javaSettings = new() { MaxRam = maxRam };
            bool createNetworkSuccess =
                await ServerManager.Instance.CreateNetworkAsync(refinedNetworkName, proxyType, javaSettings);
        }
        else
        {
            ServerVersion selectedVersion = (ServerVersion)versionComboBox.SelectedValue;
            //TODO check if inputs are valid / server not existing

            string serverName = ServerName.Text;
            string refinedServerName = serverName;
            foreach (char c in serverName)
                if (illegalDirChars.Contains(c))
                {
                    refinedServerName = refinedServerName.Replace(c + "", "");
                }

            if (refinedServerName.Equals(""))
            {
                refinedServerName = "Server";
            }

            string worldPath = null;
            if (lastPath != null)
            {
                WorldValidationInfo valInfo = DirectoryValidator.ValidateWorldDirectory(new DirectoryInfo(lastPath));
                if (valInfo.IsValid)
                {
                    worldPath = lastPath;
                }
            }

            bool createServerSuccess = await ServerManager.Instance.CreateServerAsync(refinedServerName,
                selectedVersion, viewModel.ServerSettings, new JavaSettings(), worldPath);
        }

        viewModel.GenerateNewSettings();
        CreateBtn.IsEnabled = true;

        //TODO Do something if creating fails
    }

    private void ServerDirPath_MouseDown(object sender, MouseButtonEventArgs e)
    {
        FolderBrowserDialog fbd = new();
        if (lastPath != null)
        {
            fbd.SelectedPath = lastPath;
        }

        DialogResult result = fbd.ShowDialog();
        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
        {
            worldFolderPathText.Text = fbd.SelectedPath;
            lastPath = fbd.SelectedPath;
            WorldValidationInfo valInfo =
                DirectoryValidator.ValidateWorldDirectory(new DirectoryInfo(fbd.SelectedPath));
            if (!valInfo.IsValid)
            {
                serverPathBgr.Background = (Brush)Application.Current.FindResource("buttonBgrRed");
            }
            else
            {
                serverPathBgr.Background = (Brush)Application.Current.FindResource("tabSelected");
            }
        }
    }
}