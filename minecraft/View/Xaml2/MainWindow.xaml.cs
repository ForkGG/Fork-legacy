using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.ViewModel;
using Brush = System.Windows.Media.Brush;

namespace fork.View.Xaml2
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;
        private object lastSelected;
        private bool importOpen;
        private bool createOpen;
        
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnMainWindowClose;
            viewModel = ApplicationManager.Instance.MainViewModel;
            DataContext = viewModel;
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            if (CreatePage.Visibility == Visibility.Hidden)
            {
                OpenCreateServer();
            }
            else
            {
                CloseCreateServer();
            }
        }
        private void DeleteServer_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedEntity is ServerViewModel)
            {
                DeleteServerOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteNetworkOverlay.Visibility = Visibility.Visible;
            }
        }
        private void ImportServer_Click(object sender, RoutedEventArgs e)
        {
            if (ImportPage.Visibility == Visibility.Hidden)
            {
                OpenImportServer();
            }
            else
            {
                CloseImportServer();
            }
        }

        private void OnMainWindowClose(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenCreateServer()
        {
            lastSelected = ServerList.SelectedItem;
            ServerList.UnselectAll();
            
            //Open createServer Frame
            ServerPage.Visibility = Visibility.Hidden;
            CreatePage.Visibility = Visibility.Visible;
            
            //Change Buttons
            DeleteButton.IsEnabled = false;
            ImportButton.IsEnabled = false;
            
            CreateButton.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
            CreateButton.HoverBackground = (Brush) Application.Current.FindResource("buttonBgrRed");
            CreateButton.IconSource = new BitmapImage(new Uri(@"pack://application:,,,/View/Resources/images/Icons/Cancel.png", UriKind.Absolute));
            CreateButton.HoverIconSource = new BitmapImage(new Uri(@"pack://application:,,,/View/Resources/images/Icons/CancelW.png", UriKind.Absolute));
            createOpen = true;
        }

        private void CloseCreateServer()
        {
            if (!createOpen)
            {
                return;
            }
            createOpen = false;
            
            if (ServerList.SelectedItems.Count == 0)
            {
                ServerList.SelectedItem = lastSelected;
            }
            
            //Close createServer Frame
            ServerPage.Visibility = Visibility.Visible;
            CreatePage.Visibility = Visibility.Hidden;
            
            //Change Buttons
            DeleteButton.IsEnabled = true;
            ImportButton.IsEnabled = true;
            
            CreateButton.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
            CreateButton.HoverBackground = (Brush) Application.Current.FindResource("buttonBgrGreen");
            CreateButton.IconSource = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/Create.png"));
            CreateButton.HoverIconSource = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/CreateW.png"));

        }
        
        private void OpenImportServer()
        {
            lastSelected = ServerList.SelectedItem;
            ServerList.UnselectAll();
            
            //Open importServer Frame
            ServerPage.Visibility = Visibility.Hidden;
            ImportPage.Visibility = Visibility.Visible;
            
            //Change Buttons
            DeleteButton.IsEnabled = false;
            
            CreateButton.IsEnabled = false;

            ImportButton.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
            ImportButton.HoverBackground = (Brush) Application.Current.FindResource("buttonBgrRed");
            ImportButton.IconSource = new BitmapImage(new Uri(@"pack://application:,,,/View/Resources/images/Icons/Cancel.png", UriKind.Absolute));
            ImportButton.HoverIconSource = new BitmapImage(new Uri(@"pack://application:,,,/View/Resources/images/Icons/CancelW.png", UriKind.Absolute));
            
            ImportButton.Height = CreateButton.Height;
            ImportButton.IconHeight = CreateButton.IconHeight;
            ImportButton.Width = CreateButton.Width;
            ImportButton.IconWidth = CreateButton.IconWidth;
            CreateButton.Height = DeleteButton.Height;
            CreateButton.IconHeight = DeleteButton.IconHeight *1.2;
            CreateButton.Width = DeleteButton.Width;
            CreateButton.IconWidth = DeleteButton.IconWidth *1.2;

            importOpen = true;
        }

        private void CloseImportServer()
        {
            //Check if window is already closed
            if (!importOpen)
            {
                return;
            }
            importOpen = false;
            
            if (ServerList.SelectedItems.Count == 0)
            {
                ServerList.SelectedItem = lastSelected;
            }

            //Close importServer Frame
            ServerPage.Visibility = Visibility.Visible;
            ImportPage.Visibility = Visibility.Hidden;
            
            //Change Buttons
            DeleteButton.IsEnabled = true;
            CreateButton.IsEnabled = true;

            ImportButton.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
            ImportButton.HoverBackground = (Brush) Application.Current.FindResource("buttonBgrBlue");
            ImportButton.IconSource = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/Import.png"));
            ImportButton.HoverIconSource = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/ImportW.png"));

            CreateButton.Height = ImportButton.Height;
            CreateButton.IconHeight = ImportButton.IconHeight;
            CreateButton.Width = ImportButton.Width;
            CreateButton.IconWidth = ImportButton.IconWidth;
            ImportButton.Height = DeleteButton.Height;
            ImportButton.IconHeight = DeleteButton.IconHeight;
            ImportButton.Width = DeleteButton.Width;
            ImportButton.IconWidth = DeleteButton.IconWidth;
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CreatePage.Visibility == Visibility.Visible){
                CloseCreateServer();
            }

            if (ImportPage.Visibility == Visibility.Visible)
            {
                CloseImportServer();
            }
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            DeleteServerOverlay.Visibility = Visibility.Collapsed;
            DeleteNetworkOverlay.Visibility = Visibility.Collapsed;
        }
        
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteServerOverlay.Visibility = Visibility.Collapsed;
            DeleteNetworkOverlay.Visibility = Visibility.Collapsed;

            EntityViewModel entityToDelete = viewModel.SelectedEntity;
            if (entityToDelete is ServerViewModel serverToDelete)
            {
                Application.Current.Dispatcher?.Invoke(()=>viewModel.Entities.Remove(serverToDelete), DispatcherPriority.Background); //This shouldn't be here
                bool success = await ServerManager.Instance.DeleteServerAsync(serverToDelete);
                if (!success)
                {
                    Console.WriteLine("Problem while deleting "+serverToDelete);
                    Application.Current.Dispatcher?.Invoke(()=>viewModel.Entities.Add(serverToDelete), DispatcherPriority.Background); //This shouldn't be here
                }
                else
                {
                    Console.WriteLine("Successfully deleted server "+serverToDelete);
                    ServerList.SelectedIndex = 0;
                }
            }
            else if (entityToDelete is NetworkViewModel networkToDelete)
            {
                Application.Current.Dispatcher?.Invoke(()=>viewModel.Entities.Remove(networkToDelete), DispatcherPriority.Background); //This shouldn't be here
                bool success = await ServerManager.Instance.DeleteNetworkAsync(networkToDelete);
                if (!success)
                {
                    Console.WriteLine("Problem while deleting "+networkToDelete.Network);
                    Application.Current.Dispatcher?.Invoke(()=>viewModel.Entities.Add(networkToDelete), DispatcherPriority.Background); //This shouldn't be here
                }
                else
                {
                    Console.WriteLine("Successfully deleted network "+networkToDelete.Network);
                    ServerList.SelectedIndex = 0;
                }
            }
        }

        private void EntityMouseUp(object sender, MouseButtonEventArgs e)
        {
            var s = sender as FrameworkElement;
            viewModel.SelectedEntity = s.DataContext as EntityViewModel;
        }

        private void EntityMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
