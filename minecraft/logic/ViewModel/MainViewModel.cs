using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Windows.Input;
using System.Windows.Media;
using nihilus.Annotations;
using nihilus.logic.Console;
using nihilus.logic.managers;
using nihilus.logic.model;

namespace nihilus.logic.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ServerViewModel> Servers { get; }
        public ServerViewModel SelectedServer { get; set; }

        public MainViewModel()
        {
            Servers = ServerManager.Instance.Servers;
            Servers.Insert(0, ServerViewModel.HomeViewModel());
            Servers.CollectionChanged += ServerListChanged;
            SelectedServer = Servers[0];
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