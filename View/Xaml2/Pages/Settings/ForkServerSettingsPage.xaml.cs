using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.Settings;
using Fork.View.Xaml2.Pages.Server;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Settings;

public partial class ForkServerSettingsPage : Page, ISettingsPage
{
    private readonly ServerViewModel serverViewModel;
    private readonly SettingsViewModel viewModel;

    public ForkServerSettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        SettingsFile = new SettingsFile(FileName);
        this.viewModel = viewModel;
        serverViewModel = viewModel.EntityViewModel as ServerViewModel;
        if (serverViewModel == null)
        {
            throw new Exception("ForkServerSettings was created for Network");
        }

        DataContext = serverViewModel;
        serverViewModel.Versions.CollectionChanged += (_, args) =>
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in args.NewItems)
                    if (item is ServerVersion serverVersion && serverVersion.Equals(serverViewModel.Server.Version))
                    {
                        versionComboBox.SelectedItem = item;
                    }
            }
        };
    }

    public SettingsFile SettingsFile { get; set; }
    public string FileName => "Settings";
    public string FileExtension => "";

    public async Task SaveSettings()
    {
        //serverViewModel.UpdateSettings();
    }

    private void VersionChange_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if ((bool)!versionComboBox.SelectedItem?.Equals(serverViewModel.Server.Version))
        {
            VersionChangeBtn.Visibility = Visibility.Visible;
        }
        else
        {
            VersionChangeBtn.Visibility = Visibility.Collapsed;
        }
    }

    private async void RegenNether_Click(object sender, RoutedEventArgs e)
    {
        bool success =
            await ServerManager.Instance.DeleteDimensionAsync(MinecraftDimension.Nether, serverViewModel.Server);
        DoubleAnimation doubleAnimation = new();
        doubleAnimation.From = 0.0;
        doubleAnimation.To = 0.4;
        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
        doubleAnimation.AutoReverse = true;
        Border target;
        if (success)
        {
            target = RegNetherSucc;
        }
        else
        {
            target = RegNetherFail;
        }

        Storyboard.SetTarget(doubleAnimation, target);
        Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(OpacityProperty));
        Storyboard storyboard = new();
        storyboard.Children.Add(doubleAnimation);
        storyboard.Begin();
    }

    private async void RegenEnd_Click(object sender, RoutedEventArgs e)
    {
        bool success =
            await ServerManager.Instance.DeleteDimensionAsync(MinecraftDimension.End, serverViewModel.Server);
        DoubleAnimation doubleAnimation = new();
        doubleAnimation.From = 0.0;
        doubleAnimation.To = 0.4;
        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
        doubleAnimation.AutoReverse = true;
        Border target;
        if (success)
        {
            target = RegEndSucc;
        }
        else
        {
            target = RegEndFail;
        }

        Storyboard.SetTarget(doubleAnimation, target);
        Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(OpacityProperty));
        Storyboard storyboard = new();
        storyboard.Children.Add(doubleAnimation);
        storyboard.Begin();
    }

    private async void VersionChange_Click(object sender, RoutedEventArgs e)
    {
        if (viewModel.EntityViewModel is ServerViewModel serverViewModel)
        {
            ServerPage serverPage = viewModel.EntityViewModel.EntityPage as ServerPage;
            serverPage?.OpenTerminal();
            bool success =
                await ServerManager.Instance.ChangeServerVersionAsync((ServerVersion)versionComboBox.SelectedValue,
                    serverViewModel);
            if (!success)
            {
                //TODO: Display error
            }
        }
    }
}