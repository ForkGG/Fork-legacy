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
            //Close createServer Frame
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
        
        private void OpenImportServer()
        {
            //Open importServer Frame
            ServerPage.Visibility = Visibility.Hidden;
            ImportPage.Visibility = Visibility.Visible;
            
            //Change Buttons
            DeleteButton.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
            DeleteButton.IsEnabled = false;
            
            CreateButton.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
            CreateButton.IsEnabled = false;

            ImportButton.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
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
        }

        private void CloseImportServer()
        {
            //Close importServer Frame
            ServerPage.Visibility = Visibility.Visible;
            ImportPage.Visibility = Visibility.Hidden;
            
            //Change Buttons
            DeleteButton.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
            DeleteButton.IsEnabled = true;
            CreateButton.Background = (Brush) Application.Current.FindResource("buttonBgrGreen");
            CreateButton.IsEnabled = true;
            ImportButton.Background = (Brush) Application.Current.FindResource("buttonBgrBlue");
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
            if (CreatePage.Visibility == Visibility.Visible)
            {
                CloseCreateServer();
            }

            if (ImportPage.Visibility == Visibility.Visible)
            {
                CloseImportServer();
            }
        }
    }
}
