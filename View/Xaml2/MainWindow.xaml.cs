using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.logic.Utils;
using Fork.ViewModel;
using Brush = System.Windows.Media.Brush;

namespace Fork.View.Xaml2
{
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

        private void OpenAppSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenAppSettings();
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
        private void DeleteOpen_Click(object sender, RoutedEventArgs e)
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

        private void RenameOpen_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedEntity is ServerViewModel)
            {
                RenameServerOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                RenameNetworkOverlay.Visibility = Visibility.Visible;
            }
        }

        private void CloneOpen_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedEntity is ServerViewModel serverViewModel)
            {
                if(serverViewModel.CurrentStatus == ServerStatus.STOPPED)
                {
                    Clone_Click(sender, e);
                }
                CloneServerOverlay.Visibility = Visibility.Visible;
            }
            else if(viewModel.SelectedEntity is NetworkViewModel networkViewModel)
            {
                if (networkViewModel.CurrentStatus == ServerStatus.STOPPED)
                {
                    Clone_Click(sender, e);
                }
                CloneNetworkOverlay.Visibility = Visibility.Visible;
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

        private void DiscordOpen_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://discord.fork.gg";
            ForkUtils.OpenUrl(url);
        }
        
        private void KoFiOpen_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://ko-fi.com/forkgg";
            ForkUtils.OpenUrl(url);
        }

        private void OnMainWindowClose(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenCreateServer()
        {
            if (AppSettingsPage.Visibility == Visibility.Visible)
            {
                CloseAppSettings();
            }
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
            if (AppSettingsPage.Visibility == Visibility.Visible)
            {
                CloseAppSettings();
            }
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

        private void OpenAppSettings()
        {
            CloseNonEntityPages();
            lastSelected = ServerList.SelectedItem;
            ServerList.UnselectAll();
            
            
            //TODO make loading icon or smth
            viewModel.AppSettingsViewModel.OpenAppSettingsPage();
            
            //Open importServer Frame
            ServerPage.Visibility = Visibility.Hidden;
            AppSettingsPage.Visibility = Visibility.Visible;

            //Change Buttons
            AppSettingsButton.IsEnabled = false;
            AppSettingsButton.Background = (Brush) Application.Current.FindResource("tabSelected");
        }

        private void CloseAppSettings()
        {
            //Close importServer Frame
            ServerPage.Visibility = Visibility.Visible;
            AppSettingsPage.Visibility = Visibility.Hidden;
            
            //Save settings:
            viewModel.AppSettingsViewModel.CloseAppSettingsPage();
            viewModel.UpdateInstalledJavaVersion();
            
            if (ServerList.SelectedItems.Count == 0)
            {
                ServerList.SelectedItem = lastSelected;
            }
            
            //Change Buttons
            AppSettingsButton.IsEnabled = true;
            AppSettingsButton.Background = (Brush) Application.Current.FindResource("buttonBgrDefault");
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is EntityViewModel entityViewModel)
            {
                entityViewModel.SaveSettings();
            }
            CloseNonEntityPages();
        }

        private void CloseNonEntityPages()
        {
            if (CreatePage.Visibility == Visibility.Visible){
                CloseCreateServer();
            }

            if (ImportPage.Visibility == Visibility.Visible)
            {
                CloseImportServer();
            }

            if (AppSettingsPage.Visibility == Visibility.Visible)
            {
                CloseAppSettings();
            }
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            DeleteServerOverlay.Visibility = Visibility.Collapsed;
            DeleteNetworkOverlay.Visibility = Visibility.Collapsed;
            RenameServerOverlay.Visibility = Visibility.Collapsed;
            RenameNetworkOverlay.Visibility = Visibility.Collapsed;
            CloneServerOverlay.Visibility = Visibility.Collapsed;
            CloneNetworkOverlay.Visibility = Visibility.Collapsed;
        }

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            string newName;
            if (viewModel.SelectedEntity is ServerViewModel serverViewModel)
            {
                ServerRenameBtn.IsEnabled = false;
                ServerRenameCancelBtn.IsEnabled = false;
                newName = NewServerName.Text;
                //TODO name verifier instead of this
                if (newName.Equals(""))
                {
                    newName = "ForkEntity";
                }

                bool success = await ServerManager.Instance.RenameServerAsync(serverViewModel, newName);
                if (success)
                {
                    Console.WriteLine("Successfully renamed Server to: "+newName);
                }
                else
                {
                    //TODO Show error
                    Console.WriteLine("Error renaming Server: "+serverViewModel.Name);
                }
                    
                ServerRenameBtn.IsEnabled = true;
                ServerRenameCancelBtn.IsEnabled = true;
            }
            else if (viewModel.SelectedEntity is NetworkViewModel networkViewModel)
            {
                NetworkRenameBtn.IsEnabled = false;
                NetworkRenameCancelBtn.IsEnabled = false;
                newName = NewNetworkName.Text;
                //TODO name verifier instead of this
                if (newName.Equals(""))
                {
                    newName = "ForkEntity";
                }

                bool success = await ServerManager.Instance.RenameNetworkAsync(networkViewModel, newName);
                if (success)
                {
                    Console.WriteLine("Successfully renamed Network to: "+newName);
                }
                else
                {
                    //TODO Show error
                    Console.WriteLine("Error renaming Network: "+networkViewModel.Name);
                }
                
                NetworkRenameBtn.IsEnabled = true;
                NetworkRenameCancelBtn.IsEnabled = true;
            }
            else
            {
                throw new NotImplementedException("Rename does not support this type of entity: "+viewModel.GetType());
            }
            
            Abort_Click(this, e);
        }

        private async void Clone_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedEntity is ServerViewModel serverViewModel)
            {
                ServerCloneBtn.IsEnabled = false;
                ServerCloneCancelBtn.IsEnabled = false;

                bool success = await ServerManager.Instance.CloneServerAsync(serverViewModel);
                if (success)
                {
                    Console.WriteLine("Successfully cloned Server:"+ serverViewModel.Name);
                }
                else
                {
                    //TODO Show error
                    Console.WriteLine("Error cloning Server: " + serverViewModel.Name);
                }

                ServerCloneBtn.IsEnabled = true;
                ServerCloneCancelBtn.IsEnabled = true;
            }
            else if (viewModel.SelectedEntity is NetworkViewModel networkViewModel)
            {
                NetworkCloneBtn.IsEnabled = false;
                NetworkCloneCancelBtn.IsEnabled = false;

                bool success = await ServerManager.Instance.CloneNetworkAsync(networkViewModel);
                if (success)
                {
                    Console.WriteLine("Successfully renamed Network to: " + networkViewModel.Name);
                }
                else
                {
                    //TODO Show error
                    Console.WriteLine("Error renaming Network: " + networkViewModel.Name);
                }

                NetworkCloneBtn.IsEnabled = true;
                NetworkCloneCancelBtn.IsEnabled = true;
            }
            else
            {
                throw new NotImplementedException("Rename does not support this type of entity: " + viewModel.GetType());
            }

            Abort_Click(this, e);
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            EntityViewModel entityToDelete = viewModel.SelectedEntity;
            if (entityToDelete is ServerViewModel serverToDelete)
            {
                ServerDeleteCancelBtn.IsEnabled = false;
                ServerDeleteBtn.IsEnabled = false;
                ServerDeleteBtn.Content = "Deleting...";
                
                bool success = await ServerManager.Instance.DeleteServerAsync(serverToDelete);
                if (!success)
                {
                    Console.WriteLine("Problem while deleting "+serverToDelete);
                }
                else
                {
                    Console.WriteLine("Successfully deleted server "+serverToDelete);
                    Application.Current.Dispatcher?.Invoke(()=>viewModel.Entities.Remove(serverToDelete), DispatcherPriority.Background); //This shouldn't be here
                    ServerList.SelectedIndex = 0;
                }
                
                ServerDeleteCancelBtn.IsEnabled = true;
                ServerDeleteBtn.IsEnabled = true;
                ServerDeleteBtn.Content = "Delete";
            }
            else if (entityToDelete is NetworkViewModel networkToDelete)
            {
                NetworkDeleteCancelBtn.IsEnabled = false;
                NetworkDeleteBtn.IsEnabled = false;
                NetworkDeleteBtn.Content = "Deleting...";
                
                bool success = await ServerManager.Instance.DeleteNetworkAsync(networkToDelete);
                if (!success)
                {
                    Console.WriteLine("Problem while deleting "+networkToDelete.Network);
                }
                else
                {
                    Console.WriteLine("Successfully deleted network "+networkToDelete.Network);
                    Application.Current.Dispatcher?.Invoke(()=>viewModel.Entities.Remove(networkToDelete), DispatcherPriority.Background); //This shouldn't be here
                    ServerList.SelectedIndex = 0;
                }
                
                NetworkDeleteCancelBtn.IsEnabled = true;
                NetworkDeleteBtn.IsEnabled = true;
                NetworkDeleteBtn.Content = "Delete";
            }
            
            DeleteServerOverlay.Visibility = Visibility.Collapsed;
            DeleteNetworkOverlay.Visibility = Visibility.Collapsed;
        }

        private void Ignore_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateInstalledJavaVersion(true);
        }
        
        private void CheckAgain_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateInstalledJavaVersion();
        }

        private void TextBlock_Link_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                string url = textBlock.Text;
                ForkUtils.OpenUrl(url);
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

        private void NewVersion_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string url = viewModel.LatestForkVersion.URL;
            ForkUtils.OpenUrl(url);
        }
    }
}
