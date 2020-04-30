using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using nihilus.Logic.Manager;
using nihilus.ViewModel;
using Application = System.Windows.Application;

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
            ImportServerGrid.Visibility = Visibility.Visible;
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            CreateServerGrid.Visibility = Visibility.Visible;
        }

        private void OnMainWindowClose(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void HandleCreateServerPageClose(object sernder, EventArgs e)
        {
            CloseCreateServerPage();
        }
        public void HandleImportPageClose(object sernder, EventArgs e)
        {
            CloseImportPage();
        }

        public void HandleImportPageNext(object sender, EventArgs e)
        {
            ImportFrame.SetBinding(ContentProperty, new Binding{Source = viewModel.ImportPage.SecondPage});
        }
        public void HandleImportPagePrevious(object sender, EventArgs e)
        {
            ImportFrame.SetBinding(ContentProperty, new Binding{Source = viewModel.ImportPage});
        }

        private void CloseCreateServerPage()
        {
            CreateServerGrid.Visibility = Visibility.Collapsed;
        }
        private void CloseImportPage()
        {
            ImportServerGrid.Visibility = Visibility.Collapsed;
        }
    }
}