using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using fork.Annotations;
using fork.Logic.ApplicationConsole;
using fork.Logic.Manager;
using fork.View.Xaml2.Pages;

namespace fork.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private ImportPage importPage;

        //public MainWindow MainWindow { get; set; }
        
        public ObservableCollection<EntityViewModel> Entities { get; set; }
        public EntityViewModel SelectedEntity { get; set; }
        public ImportViewModel ImportViewModel { get; set; }
        public bool HasServers { get; set; }
        
        public CreatePage CreatePage { get; } = new CreatePage();
        public ImportPage ImportPage { get; } = new ImportPage();
        

        public MainViewModel()
        {
            //Writes console to Application Console
            //Console.SetOut(ApplicationManager.ConsoleWriter);
            
            ImportViewModel = new ImportViewModel();
            Entities = ServerManager.Instance.Entities;
            //Servers.Insert(0, ServerViewModel.HomeViewModel());
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}