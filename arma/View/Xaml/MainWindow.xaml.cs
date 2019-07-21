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