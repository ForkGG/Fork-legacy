using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using Fork.Annotations;
using Fork.Logic.Controller;
using Fork.Logic.Manager;
using Fork.Logic.Model.APIModels;
using Fork.View.Xaml2.Pages;
using Timer = System.Timers.Timer;

namespace Fork.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private ImportPage importPage;

        //public MainWindow MainWindow { get; set; }
        
        public ObservableCollection<EntityViewModel> Entities { get; set; }
        public EntityViewModel SelectedEntity { get; set; }
        //public ImportViewModel ImportViewModel { get; }
        public AppSettingsViewModel AppSettingsViewModel { get; }
        public bool HasServers { get; set; }
        public bool NewerVersionExists { get; set; }
        public bool IsBetaVersion => CurrentForkVersion.Beta != 0;
        public ForkVersion CurrentForkVersion { get; set; }
        public ForkVersion LatestForkVersion { get; set; }
        
        public CreatePage CreatePage { get; } = new CreatePage();
        public ImportPage ImportPage { get; } = new ImportPage();
        

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
            //ImportViewModel = new ImportViewModel();
            AppSettingsViewModel = new AppSettingsViewModel(this);
            Entities = ServerManager.Instance.Entities;
            Entities.CollectionChanged += ServerListChanged;
            if (Entities.Count != 0)
            {
                SelectedEntity = Entities[0];
                HasServers = true;
            }
        }

        public void SetServerList(ref ObservableCollection<EntityViewModel> entities)
        {
            Entities = entities;
            HasServers = Entities.Count!=0;
            Entities.CollectionChanged += ServerListChanged;
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
            Timer timer = new Timer {Interval = 1000 * 60 * 60 * 12, AutoReset = true, Enabled = true};
            timer.Elapsed += OnVersionTimerElapsed;
        }

        private void OnVersionTimerElapsed(object source, ElapsedEventArgs e)
        {
            CheckForkVersion();
        }

        private void CheckForkVersion()
        {
            LatestForkVersion = new APIController().GetLatestForkVersion();
            if (LatestForkVersion.CompareTo(CurrentForkVersion)>0)
            {
                NewerVersionExists = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}