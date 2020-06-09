using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.View.Xaml2.Pages.Server;
using fork.ViewModel;

namespace fork.View.Xaml2.Pages.Settings
{
    public partial class VanillaSettingsPage : Page
    {
        private SettingsViewModel viewModel;
        private ServerViewModel serverViewModel;
        
        public VanillaSettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            serverViewModel = viewModel.EntityViewModel as ServerViewModel;
            DataContext = serverViewModel;
        }

        private void VersionChange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((bool) !versionComboBox.SelectedItem?.Equals(serverViewModel.Server.Version))
            {
                VersionChangeBtn.Visibility = Visibility.Visible;
            }
            else
            {
                VersionChangeBtn.Visibility = Visibility.Collapsed; 
            }
        }

        private async void RegenNether_Click(object sender, RoutedEventArgs e)
        {
            bool success = await ServerManager.Instance.DeleteDimensionAsync(MinecraftDimension.Nether, serverViewModel.Server);
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.0;
            doubleAnimation.To = 0.4;
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            doubleAnimation.AutoReverse = true;
            Border target;
            if (success)
            {
                target = RegNetherSucc;
            }
            else
            {
                target = RegNetherFail;
            }
            Storyboard.SetTarget(doubleAnimation, target);
            Storyboard.SetTargetProperty(doubleAnimation,new PropertyPath(OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();
        }

        private async void RegenEnd_Click(object sender, RoutedEventArgs e)
        {
            bool success = await ServerManager.Instance.DeleteDimensionAsync(MinecraftDimension.End, serverViewModel.Server);
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.0;
            doubleAnimation.To = 0.4;
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            doubleAnimation.AutoReverse = true;
            Border target;
            if (success)
            {
                target = RegEndSucc;
            }
            else
            {
                target = RegEndFail;
            }
            Storyboard.SetTarget(doubleAnimation, target);
            Storyboard.SetTargetProperty(doubleAnimation,new PropertyPath(OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();
        }

        private async void VersionChange_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.EntityViewModel is ServerViewModel serverViewModel)
            {
                ServerPage serverPage = viewModel.EntityViewModel.EntityPage as ServerPage;
                serverPage?.OpenTerminal();
                bool success = await ServerManager.Instance.ChangeServerVersionAsync((ServerVersion) versionComboBox.SelectedValue, serverViewModel);
                if (!success)
                {
                    //TODO: Display error
                    return;
                }
            }
        }
    }
}
