
using System.Windows;
using nihilus.ViewModel;

namespace nihilus.View.Xaml
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private ServerViewModel viewModel;
        
        public SettingsWindow(ServerViewModel viewModel)
        {
            this.viewModel = viewModel;
            DataContext = this.viewModel.Server;
            InitializeComponent();
        }

        private void Btn_Apply(object sender, RoutedEventArgs e)
        {
            Close();
            viewModel.UpdateSettings();
        }
    }
}
