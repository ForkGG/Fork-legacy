using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Fork.Logic.Controller;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Persistence;
using Fork.View.Xaml2.Pages;
using Websocket.Client;

namespace Fork.ViewModel;

public class AppSettingsViewModel : BaseViewModel
{
    private string oldDefaultJavaPath;
    private readonly Timer retryTimer = new() { Interval = 1000, AutoReset = true, Enabled = false };

    public AppSettingsViewModel(MainViewModel mainViewModel)
    {
        retryTimer.Elapsed += (sender, e) =>
        {
            if (RetrySeconds > 0)
            {
                RetrySeconds--;
            }
        };

        if (AppSettings.EnableDiscordBot)
        {
            ApplicationManager.StartDiscordWebSocket();
        }

        MainViewModel = mainViewModel;
        AppSettingsPage = new AppSettingsPage(this);
        Supporters = new ObservableCollection<string>(new APIController().GetSupporters());
    }

    public AppSettings AppSettings => AppSettingsSerializer.Instance.AppSettings;

    public string DiscordSocketStateMessage
    {
        get
        {
            if (!IsDiscordBotConnected)
            {
                return "Disconnected";
            }

            return !IsDiscordLinked ? "Waiting for token" : "Connected";
        }
    }

    public bool IsDiscordBotConnected { get; set; }
    public bool IsDiscordLinked { get; set; }
    public int RetrySeconds { get; set; } = 30;
    public string DiscordGuildName { get; set; } = "";
    public AppSettingsPage AppSettingsPage { get; }
    public MainViewModel MainViewModel { get; }
    public ObservableCollection<string> Supporters { get; set; }

    public async Task OpenAppSettingsPage()
    {
        await ReadAppSettingsAsync();
        oldDefaultJavaPath = AppSettings.DefaultJavaPath;
    }

    public async Task CloseAppSettingsPage()
    {
        if (oldDefaultJavaPath != null && !oldDefaultJavaPath.Equals(AppSettings.DefaultJavaPath))
        {
            foreach (EntityViewModel entityViewModel in MainViewModel.Entities)
                entityViewModel.UpdateDefaultJavaPath(oldDefaultJavaPath, AppSettings.DefaultJavaPath);
        }

        await WriteAppSettingsAsync();
    }

    public void UpdateDiscordWebSocketState(ReconnectionType type)
    {
        IsDiscordBotConnected = true;
        retryTimer.Stop();
        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(DiscordSocketStateMessage)));
    }

    public void UpdateDiscordWebSocketState(DisconnectionType type)
    {
        IsDiscordBotConnected = false;
        IsDiscordLinked = false;
        RetrySeconds = 30;
        retryTimer.Start();
        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(DiscordSocketStateMessage)));
    }

    public void DiscordLinkStatusUpdate(string status, string guildName = "")
    {
        IsDiscordLinked = status.ToLower().Equals("linked");
        DiscordGuildName = guildName;
        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(DiscordSocketStateMessage)));
    }

    public async Task<bool> WriteAppSettingsAsync()
    {
        Task<bool> t = new(() =>
        {
            try
            {
                AppSettingsSerializer.Instance.SaveSettings();
                return true;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return false;
            }
        });
        t.Start();
        bool r = await t;
        return r;
    }

    private async Task<bool> ReadAppSettingsAsync()
    {
        Task<bool> t = new(() =>
        {
            try
            {
                ObservableCollection<string> patronsNew =
                    new ObservableCollection<string>(new APIController().GetSupporters());
                Application.Current.Dispatcher.InvokeAsync(() => { Supporters = patronsNew; });
                AppSettingsSerializer.Instance.ReadSettings();
                return true;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                return false;
            }
        });
        t.Start();
        bool r = await t;
        RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(AppSettings)));
        return r;
    }
}