using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fork.Annotations;
using Fork.Logic.Controller;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Model.APIModels;
using Fork.Logic.Utils;
using Fork.View.Xaml2.Pages;
using Timer = System.Timers.Timer;

namespace Fork.ViewModel;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private ImportPage importPage;


    public MainViewModel()
    {
        if (!ApplicationManager.Initialized)
        {
            ApplicationManager.ApplicationInitialized += SetupVersionChecking;
        }
        else
        {
            SetupVersionChecking();
        }

        AppSettingsViewModel = new AppSettingsViewModel(this);
        UpdateInstalledJavaVersion();
        Entities = ServerManager.Instance.Entities;
        Entities.CollectionChanged += ServerListChanged;
        if (Entities.Count != 0)
        {
            SelectedEntity = Entities[0];
            HasServers = true;
        }

        Boi = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/BoiTransparent.png"));
        BoiHover = new BitmapImage(
            new Uri("pack://application:,,,/View/Resources/images/Icons/BoiTransparentHover.png"));

        DateTime now = DateTime.Now;
        if (now.Month == 12 && now.Day < 27)
        {
            Boi = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/XMasBoiTransparent.png"));
            BoiHover = new BitmapImage(
                new Uri("pack://application:,,,/View/Resources/images/Icons/XMasBoiTransparentHover.png"));
        }
    }

    public ObservableCollection<EntityViewModel> Entities { get; set; }
    public EntityViewModel SelectedEntity { get; set; }
    public AppSettingsViewModel AppSettingsViewModel { get; }
    public bool HasServers { get; set; }
    public bool NewerVersionExists { get; set; }
    public bool IsBetaVersion => CurrentForkVersion.Beta != 0;
    public ForkVersion CurrentForkVersion { get; set; }
    public ForkVersion LatestForkVersion { get; set; }
    public bool IsLatestBeta => LatestForkVersion.Beta != 0;
    public JavaVersion InstalledJavaVersion { get; private set; }
    public bool ShowJavaWarning { get; private set; }
    public string JavaWarningMessage { get; private set; }
    public ImageSource Boi { get; private set; }
    public ImageSource BoiHover { get; private set; }


    public CreatePage CreatePage { get; } = new();
    public ImportPage ImportPage { get; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public void UpdateInstalledJavaVersion(bool ignoreWarnings = false)
    {
        InstalledJavaVersion = JavaVersionUtils.GetInstalledJavaVersion();
        if (InstalledJavaVersion == null)
        {
            ShowJavaWarning = !ignoreWarnings;
            JavaWarningMessage =
                "No Java installation detected!" +
                "\nMinecraft Servers require Java to be installed on your system";
            return;
        }

        if (!InstalledJavaVersion.Is64Bit)
        {
            ShowJavaWarning = !ignoreWarnings;
            JavaWarningMessage =
                "32-Bit Java installation detected!" +
                "\nThis will cause issues if you want to use more than 1GB of RAM";
            return;
        }

        if (InstalledJavaVersion.VersionComputed < 17)
        {
            ShowJavaWarning = !ignoreWarnings;
            JavaWarningMessage =
                "Old Java installation detected (Version " + InstalledJavaVersion.VersionComputed + ")!" +
                "\nOlder Java versions will cause problems from Minecraft 1.17 onwards." +
                "\nWe recommend installing Java version 21 or higher for full support and best performance";
            return;
        }

        ShowJavaWarning = false;
        JavaWarningMessage = "";
    }

    public void SetServerList(ref ObservableCollection<EntityViewModel> entities)
    {
        Entities = entities;
        HasServers = Entities.Count != 0;
        Entities.CollectionChanged += ServerListChanged;
    }

    public void CheckForkVersion()
    {
        LatestForkVersion = new APIController().GetLatestForkVersion(AppSettingsViewModel.AppSettings.UseBetaVersions);
        if (LatestForkVersion.CompareTo(CurrentForkVersion) > 0)
        {
            NewerVersionExists = true;
            raisePropertyChanged(nameof(IsLatestBeta));
        }
        else
        {
            NewerVersionExists = false;
        }
    }

    private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        raisePropertyChanged(nameof(Entities));
    }

    private void SetupVersionChecking()
    {
        CurrentForkVersion = ApplicationManager.Instance.CurrentForkVersion;
        NewerVersionExists = false;
        CheckForkVersion();
        Timer timer = new() { Interval = 1000 * 60 * 60 * 12, AutoReset = true, Enabled = true };
        timer.Elapsed += OnVersionTimerElapsed;
    }

    private void OnVersionTimerElapsed(object source, ElapsedEventArgs e)
    {
        CheckForkVersion();
    }

    [NotifyPropertyChangedInvocator]
    private void raisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}