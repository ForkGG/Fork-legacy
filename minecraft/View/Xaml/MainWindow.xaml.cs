using System;
using System.ComponentModel;
using System.Windows;
using nihilus.Logic.Manager;
using nihilus.ViewModel;

namespace nihilus.xaml
{
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnMainWindowClose;
            viewModel = ApplicationManager.Instance.MainViewModel;
            viewModel.MainWindow = this;
            DataContext = viewModel;
        }

        private void ImportServer_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            CreateServerGrid.Visibility = Visibility.Visible;
            //AddServer.AddServer addServer = new AddServer.AddServer();
            //addServer.ShowDialog();
        }

        private void OnMainWindowClose(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void HandleCreateServerPageClose(object sernder, EventArgs e)
        {
            CloseCreateServerPage();
        }

        private void CloseCreateServerPage()
        {
            CreateServerGrid.Visibility = Visibility.Collapsed;
        }
    }
}