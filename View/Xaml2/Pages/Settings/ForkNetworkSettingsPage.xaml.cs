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
using System.Windows.Navigation;
using System.Windows.Shapes;
using fork;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Model.Settings;
using fork.View.Xaml2.Pages.Server;
using fork.ViewModel;
using Path = System.IO.Path;

namespace Fork.View.Xaml2.Pages.Settings
{
    /// <summary>
    /// Interaktionslogik für ForkNetworkSettingsPage.xaml
    /// </summary>
    public partial class ForkNetworkSettingsPage : Page, ISettingsPage
    {
        private SettingsViewModel viewModel;
        private NetworkViewModel networkViewModel;
        
        public ForkNetworkSettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            SettingsFile = new SettingsFile(FileName);
            this.viewModel = viewModel;
            networkViewModel = viewModel.EntityViewModel as NetworkViewModel;
            if (networkViewModel == null)
            {
                throw new Exception("ForkNetworkSettings was created for Server");
            }

            DataContext = networkViewModel;
        }

        public SettingsFile SettingsFile { get; set; }
        public string FileName => "Settings";
        public string FileExtension => "";
        public void SaveSettings()
        {
            //networkViewModel.SaveSettings();
        }
    }
}
