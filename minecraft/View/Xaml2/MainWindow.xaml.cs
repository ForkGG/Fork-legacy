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
using nihilus.Logic.Manager;
using nihilus.ViewModel;
using Brush = System.Windows.Media.Brush;

namespace nihilus.View.Xaml2
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;
        
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
            
        }
        private void ImportServer_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnMainWindowClose(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenCreateServer()
        {
            //Open createServer Frame
            ServerPage.Visibility = Visibility.Hidden;
            CreatePage.Visibility = Visibility.Visible;
            
            //Change Buttons
            DeleteButton.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
            DeleteButton.IsEnabled = false;
            ImportButton.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
            ImportButton.IsEnabled = false;
            CreateButton.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
            CreateButton.IconSource = new BitmapImage(new Uri(@"pack://application:,,,/View/Resources/images/Icons/Cancel.png", UriKind.Absolute));
            CreateButton.HoverIconSource = new BitmapImage(new Uri(@"pack://application:,,,/View/Resources/images/Icons/CancelW.png", UriKind.Absolute));
        }

        private void CloseCreateServer()
        {
            //Open createServer Frame
            ServerPage.Visibility = Visibility.Visible;
            CreatePage.Visibility = Visibility.Hidden;
            
            //Change Buttons
            DeleteButton.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
            DeleteButton.IsEnabled = true;
            ImportButton.Background = (Brush) Application.Current.FindResource("buttonBgrBlue");
            ImportButton.IsEnabled = true;
            CreateButton.Background = (Brush) Application.Current.FindResource("buttonBgrGreen");
            CreateButton.IconSource = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/Create.png"));
            CreateButton.HoverIconSource = new BitmapImage(new Uri("pack://application:,,,/View/Resources/images/Icons/CreateW.png"));
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CreatePage.Visibility == Visibility.Visible)
            {
                CloseCreateServer();
            }
        }
    }
}
