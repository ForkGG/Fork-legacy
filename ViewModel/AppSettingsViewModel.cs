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

namespace Fork.ViewModel
{
    public class AppSettingsViewModel : BaseViewModel
    {
        private string oldDefaultJavaPath;
        private Timer retryTimer = new Timer {Interval = 1000, AutoReset = true, Enabled = false};
        
        public AppSettings AppSettings => AppSettingsSerializer.Instance.AppSettings;
        public string DiscordSocketStateMessage { get; set; } = "Not connected";
        public bool IsDiscordBotConnected { get; set; } = false;
        public int RetrySeconds { get; set; } = 30;
        public AppSettingsPage AppSettingsPage { get; }
        public MainViewModel MainViewModel { get; }
        public ObservableCollection<string> Patrons { get; set; }

        public AppSettingsViewModel(MainViewModel mainViewModel)
        {
            retryTimer.Elapsed += (sender, e) =>
            {
                if (RetrySeconds > 0)
                {
                    RetrySeconds--;
                }
            };
            
            MainViewModel = mainViewModel;
            AppSettingsPage = new AppSettingsPage(this);
            Patrons = new ObservableCollection<string>(new APIController().GetPatrons());
        }

        public async Task OpenAppSettingsPage()
        {
            await ReadAppSettingsAsync();
            oldDefaultJavaPath = AppSettings.DefaultJavaPath;
        }

        public async Task CloseAppSettingsPage()
        {
            if (oldDefaultJavaPath!=null && !oldDefaultJavaPath.Equals(AppSettings.DefaultJavaPath))
            {
                foreach (EntityViewModel entityViewModel in MainViewModel.Entities)
                {
                    entityViewModel.UpdateDefaultJavaPath(oldDefaultJavaPath, AppSettings.DefaultJavaPath);
                }
                EntitySerializer.Instance.StoreEntities(ServerManager.Instance.Entities);
            }
            await WriteAppSettingsAsync();
        }

        public void UpdateDiscordWebSocketState(ReconnectionType type)
        {
            DiscordSocketStateMessage = "Your Fork instance is connected to the Discord Bot";
            IsDiscordBotConnected = true;
            retryTimer.Stop();
        }
        
        public void UpdateDiscordWebSocketState(DisconnectionType type)
        {
            DiscordSocketStateMessage = "Your Fork instance is not connected to the Discord Bot";
            IsDiscordBotConnected = false;
            RetrySeconds = 30;
            retryTimer.Start();
        }

        private async Task<bool> WriteAppSettingsAsync()
        {
            Task<bool> t = new Task<bool>(() =>
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
            Task<bool> t = new Task<bool>(() =>
            {
                try
                {
                    var patronsNew = new ObservableCollection<string>(new APIController().GetPatrons());
                    Application.Current.Dispatcher.InvokeAsync(() => { Patrons = patronsNew; });
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
}