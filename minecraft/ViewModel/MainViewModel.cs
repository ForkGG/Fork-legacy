using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using nihilus.Annotations;
using nihilus.Logic.ApplicationConsole;
using nihilus.Logic.Manager;
using nihilus.View.Xaml.Pages;
using nihilus.xaml;

namespace nihilus.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private CreateServerPage createServerPage;

        public MainWindow MainWindow { get; set; }
        public ObservableCollection<ServerViewModel> Servers { get; }
        public ServerViewModel SelectedServer { get; set; }

        public CreateServerPage CreateServerPage
        {
            get
            {
                if (createServerPage == null)
                {
                    GenerateCreateServerPage();
                }
                return createServerPage;
            }
        }

        public MainViewModel()
        {
            Console.SetOut(ApplicationManager.ConsoleWriter);
            Servers = ServerManager.Instance.Servers;
            Servers.Insert(0, ServerViewModel.HomeViewModel());
            Servers.CollectionChanged += ServerListChanged;
            SelectedServer = Servers[0];
        }


        private void GenerateCreateServerPage()
        {
            createServerPage = new CreateServerPage();
            createServerPage.CreateServerCloseEvent += HandleCreateServerClose;
            createServerPage.CreateServerCloseEvent += MainWindow.HandleCreateServerPageClose;
            raisePropertyChanged(nameof(createServerPage));
        }

        private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(Servers));
        }

        private void HandleCreateServerClose(object sender, EventArgs e)
        {
            GenerateCreateServerPage();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}