using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Threading;
using fork.Annotations;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Model.Settings;
using fork.View.Xaml2.Pages.Settings;
using Fork.View.Xaml2.Pages.Settings;
using ICSharpCode.AvalonEdit.Rendering;

namespace fork.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        #region private Varibles
        
        

        #endregion

        #region Properties
        
        //Others
        public Entity Entity { get; set; }
        //public ObservableCollection<SettingsFile> Settings { get; set; }
        public EntityViewModel EntityViewModel { get; set; }

        //Pages
        public SettingsPage SettingsPage { get; }
        //public VanillaSettingsPage VanillaSettingsPage { get; }
        //public ProxySettingsPage ProxySettingsPage { get; }
        public ObservableCollection<ISettingsPage> SettingsPages { get; set; } = new ObservableCollection<ISettingsPage>();

        #endregion

        public SettingsViewModel(ServerViewModel serverViewModel)
        {
            EntityViewModel = serverViewModel;
            Entity = serverViewModel.Entity;
            SettingsPage = new SettingsPage(this);
            //VanillaSettingsPage = new VanillaSettingsPage(this);
        }

        public SettingsViewModel(NetworkViewModel networkViewModel)
        {
            EntityViewModel = networkViewModel;
            Entity = networkViewModel.Network;
            SettingsPage = new SettingsPage(this);
            //ProxySettingsPage = new ProxySettingsPage(this);
        }

        public void InitializeSettings(List<SettingsFile> settingsFiles)
        {
            SettingsPages = new ObservableCollection<ISettingsPage>();
            List<SettingsFile> orderedList = settingsFiles.OrderBy(x => x.NameID).ThenByDescending(x => x.FileInfo.Name).ToList();
            foreach (SettingsFile settingsFile in orderedList)
            {
                switch (settingsFile.Type)
                {
                    case SettingsFile.SettingsType.Vanilla:
                        Application.Current.Dispatcher?.Invoke(() =>
                            SettingsPages.Add(new VanillaSettingsPage(this, settingsFile)));
                        break;
                    case SettingsFile.SettingsType.Bungee:
                        Application.Current.Dispatcher?.Invoke(() =>
                            SettingsPages.Add(new ProxySettingsPage(this, settingsFile)));
                        break;
                    default:
                        Application.Current.Dispatcher?.Invoke(() =>
                            SettingsPages.Add(new DefaultSettingsPage(settingsFile)));
                        break;
                }
            }
        }

        public void UpdateSettings(List<SettingsFile> settingsFiles)
        {
            foreach (SettingsFile settingsFile in settingsFiles)
            {
                bool add = true;
                foreach (ISettingsPage setting in SettingsPages)
                {
                    if (settingsFile.FileInfo.FullName.Equals(setting.SettingsFile.FileInfo.FullName))
                    {
                        add = false;
                        break;
                    }
                }

                if (add)
                {
                    switch (settingsFile.Type)
                    {
                        case SettingsFile.SettingsType.Vanilla:
                            Application.Current.Dispatcher?.Invoke(() =>
                                SettingsPages.Add(new VanillaSettingsPage(this, settingsFile)));
                            break;
                        case SettingsFile.SettingsType.Bungee:
                            Application.Current.Dispatcher?.Invoke(() =>
                                SettingsPages.Add(new ProxySettingsPage(this, settingsFile)));
                            break;
                        default:
                            Application.Current.Dispatcher?.Invoke(() =>
                                SettingsPages.Add(new DefaultSettingsPage(settingsFile)));
                            break;
                    }
                }

                bool remove = true;
                int removeIndex = 0;
                foreach (ISettingsPage setting in SettingsPages)
                {
                    if (settingsFile.FileInfo.FullName.Equals(setting.SettingsFile.FileInfo.FullName))
                    {
                        remove = false;
                        removeIndex = SettingsPages.IndexOf(setting);
                    }
                }

                if (remove)
                {
                    Application.Current.Dispatcher?.Invoke(() => SettingsPages.RemoveAt(removeIndex));
                }
            }
            SortSettingsPages();
        }

        private void SortSettingsPages()
        {
            List<ISettingsPage> sorted = SettingsPages.OrderBy(x => x.SettingsFile.NameID).ThenByDescending(x => x.SettingsFile.FileInfo.Name).ToList();
            
            int ptr = 0;
            while (ptr < sorted.Count)
            {
                if (!SettingsPages[ptr].Equals(sorted[ptr]))
                {
                    ISettingsPage t = SettingsPages[ptr];
                    int ptr1 = ptr;
                    Application.Current.Dispatcher?.Invoke(()=>SettingsPages.Move(ptr1,sorted.IndexOf(t)));
                }
                else
                {
                    ptr++;
                }
            }
        }

        public void SaveChanges()
        {
            foreach (ISettingsPage settingsPage in SettingsPages)
            {
                settingsPage.SaveSettings();
            }
        }
        
        
        [NotifyPropertyChangedInvocator]
        protected void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}