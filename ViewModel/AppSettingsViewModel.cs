using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Fork.Logic.Controller;
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
        public ObservableCollection<string> Patrons { get; set; }


        public delegate void HandlePageClose(object sender);
        public event HandlePageClose AppSettingsCloseEvent;

        public AppSettingsViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            AppSettingsPage = new AppSettingsPage(this);
            Patrons = new ObservableCollection<string>(new APIController().GetPatrons());
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
                    var patronsNew = new ObservableCollection<string>(new APIController().GetPatrons());
                    Application.Current.Dispatcher.InvokeAsync(() => { Patrons = patronsNew; });
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