using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Fork.Logic.Model.Settings;
using Fork.Logic.Persistence;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Settings
{
    public partial class ForkNetworkSettingsPage : Page, ISettingsPage
    {
        private readonly NetworkViewModel networkViewModel;
        private SettingsViewModel viewModel;

        public ForkNetworkSettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            SettingsFile = new SettingsFile(FileName);
            this.viewModel = viewModel;
            networkViewModel = viewModel.EntityViewModel as NetworkViewModel;

            DataContext = networkViewModel ?? throw new Exception("ForkNetworkSettings was created for Server");
        }

        public SettingsFile SettingsFile { get; set; }
        public string FileName => "Settings";
        public string FileExtension => "";

        public async Task SaveSettings()
        {
            //networkViewModel.SaveSettings();
        }

        private void JavaPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
                {Multiselect = false, Filter = "Java executable|java.exe", Title = "Select a java.exe"};
            if (!ServerJavaPath.Text.Equals("java.exe") &&
                new DirectoryInfo(ServerJavaPath.Text.Replace(@"\java.exe", "")).Exists)
                ofd.InitialDirectory = ServerJavaPath.Text.Replace(@"\java.exe", "");
            else
                ofd.InitialDirectory = @"C:\Program Files";

            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                ServerJavaPath.Text = ofd.FileName;
        }

        private void DefaultJavaDirReset_Click(object sender, RoutedEventArgs e)
        {
            ServerJavaPath.Text = AppSettingsSerializer.Instance.AppSettings.DefaultJavaPath;
        }
    }
}