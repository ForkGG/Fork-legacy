
using System;
using System.Windows;
using System.IO.Compression;
using nihilus.Logic.Manager;
using nihilus.Logic.Model;
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
            DataContext = this.viewModel;
            InitializeComponent();
        }

        private void Btn_Apply(object sender, RoutedEventArgs e)
        {
            Close();
            viewModel.UpdateSettings();
        }

        private async void Btn_Delete(object sender, RoutedEventArgs e)
        {
            Close();
            bool success = await ServerManager.Instance.DeleteServerAsync(viewModel);
            if(!success)
                Console.WriteLine("Problem while deleting "+viewModel.Server.Name);
            else
            {
                ApplicationManager.Instance.MainViewModel.Servers.Remove(viewModel); //This shouldn't be here
            }
        }

        private async void Btn_ChangeVersion(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            bool success = await ServerManager.Instance.ChangeServerVersionAsync((ServerVersion) VersionSelector.SelectedValue, viewModel);
            if (!success)
            {
                //TODO: Display error
                return;
            }
            
            Close();          
        }

        private void Btn_RegenerateEnd(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
            //TODO: Delete and save DIM1
        }

        private void Btn_RegenerateNether(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
            //TODO: Delete and save DIM-1
        }
    }
}
