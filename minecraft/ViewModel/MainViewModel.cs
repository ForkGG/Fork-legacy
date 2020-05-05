using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using nihilus.Annotations;
using nihilus.Logic.ApplicationConsole;
using nihilus.Logic.Manager;
using nihilus.View.Xaml.Pages;
using nihilus.View.Xaml2.Pages;
using nihilus.xaml;

namespace nihilus.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private CreateServerPage createServerPage;
        private ImportPage importPage;

        public MainWindow MainWindow { get; set; }
        public ObservableCollection<ServerViewModel> Servers { get; }
        public ServerViewModel SelectedServer { get; set; }
        public ImportViewModel ImportViewModel { get; set; }
        
        public CreatePage CreatePage { get; } = new CreatePage();
        
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
        
        public ImportPage ImportPage
        {
            get
            {
                if (importPage == null)
                {
                    GenerateImportPage();
                }
                return importPage;
            }
        }

        public MainViewModel()
        {
            //Writes console to Application Console
            Console.SetOut(ApplicationManager.ConsoleWriter);
            
            ImportViewModel = new ImportViewModel();
            Servers = ServerManager.Instance.Servers;
            //Servers.Insert(0, ServerViewModel.HomeViewModel());
            Servers.CollectionChanged += ServerListChanged;
            SelectedServer = Servers[0];
        }

        private void GenerateCreateServerPage()
        {
            createServerPage = new CreateServerPage();
            createServerPage.CreateServerCloseEvent += HandleCreateServerClose;
            createServerPage.CreateServerCloseEvent += MainWindow.HandleCreateServerPageClose;
            raisePropertyChanged(nameof(CreateServerPage));
        }
        
        private void GenerateImportPage()
        {
            ImportViewModel.RegenerateServerSettings();
            importPage = new ImportPage(ImportViewModel);
            ImportViewModel.ImportCloseEvent += HandleImportClose;
            ImportViewModel.ImportCloseEvent += MainWindow.HandleImportPageClose;
            ImportViewModel.ImportNextEvent += MainWindow.HandleImportPageNext;
            ImportViewModel.ImportPreviousEvent += MainWindow.HandleImportPagePrevious;
            raisePropertyChanged(nameof(ImportPage));
        }

        private void ServerListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            raisePropertyChanged(nameof(Servers));
        }

        private void HandleCreateServerClose(object sender, EventArgs e)
        {
            GenerateCreateServerPage();
        }
        private void HandleImportClose(object sender, EventArgs e)
        {
            GenerateImportPage();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void raisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}