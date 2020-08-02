using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.Logic.Persistence;
using Fork.View.Xaml2.Pages;

namespace Fork.ViewModel
{
    public class AppSettingsViewModel : BaseViewModel
    {
        public AppSettings AppSettings => AppSettingsSerializer.AppSettings;
        public AppSettingsPage AppSettingsPage { get; }
        public MainViewModel MainViewModel { get; }


        public delegate void HandlePageClose(object sender);
        public event HandlePageClose AppSettingsCloseEvent;

        public AppSettingsViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            AppSettingsPage = new AppSettingsPage(this);
        }

        public void ClosePage(object sender)
        {
            AppSettingsCloseEvent?.Invoke(sender);
            SaveAppSettingsAsync();
        }

        public async Task<bool> SaveAppSettingsAsync()
        {
            Task<bool> t = new Task<bool>(() =>
            {
                try
                {
                    AppSettingsSerializer.SaveSettings();
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

        public async Task<bool> ReadAppSettingsAsync()
        {
            Task<bool> t = new Task<bool>(() =>
            {
                try
                {
                    AppSettingsSerializer.ReadSettings();
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