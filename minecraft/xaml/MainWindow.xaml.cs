using System;
using System.Windows;
using System.Windows.Controls;
using LiveCharts.Charts;
using nihilus.logic;
using nihilus.logic.managers;
using nihilus.logic.model;
using nihilus.logic.ViewModel;
using nihilus.xaml.AddServer;

namespace nihilus.xaml
{
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = ApplicationManager.Instance.MainViewModel;
            DataContext = viewModel;
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            AddServer.AddServer addServer = new AddServer.AddServer();
            addServer.ShowDialog();
        }
    }
}