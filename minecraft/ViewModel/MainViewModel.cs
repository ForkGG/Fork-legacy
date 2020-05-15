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
        
        public ObservableCollection<ServerViewModel> Servers { get; set; }
        public ServerViewModel SelectedServer { get; set; }
        public ImportViewModel ImportViewModel { get; set; }
        public bool HasServers { get; set; }
        
        public CreatePage CreatePage { get; } = new CreatePage();
        public ImportPage ImportPage { get; } = new ImportPage();
        

        public MainViewModel()
        {
            //Writes console to Application Console
            //Console.SetOut(ApplicationManager.ConsoleWriter);
            
            ImportViewModel = new ImportViewModel();
            Servers = ServerManager.Instance.Servers;
            //Servers.Insert(0, ServerViewModel.HomeViewModel());
            Servers.CollectionChanged += ServerListChanged;
            if (Servers.Count != 0)
            {
                SelectedServer = Servers[0];
                HasServers = true;
            }
        }

        public void SetServerList(ref ObservableCollection<ServerViewModel> servers)
        {
            Servers = servers;
            HasServers = Servers.Count!=0;
            Servers.CollectionChanged += ServerListChanged;
        }

        private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(Servers));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}