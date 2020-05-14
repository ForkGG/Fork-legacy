using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using fork;
using fork.Logic.Model;
using fork.ViewModel;

namespace Fork.View.Xaml2.Pages
{
    public partial class WorldsPage : Page
    {
        private ServerViewModel viewModel;
        
        public WorldsPage(ServerViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
        }

        private void OpenExplorer_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string path = Path.Combine(App.ApplicationPath,viewModel.Server.Name);
            Process.Start("explorer.exe", "-p, " + path);
        }
    }
}