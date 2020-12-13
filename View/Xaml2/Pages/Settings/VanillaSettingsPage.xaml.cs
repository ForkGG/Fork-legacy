using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Fork.Logic.Model.Settings;
using Fork.ViewModel;
using Path = System.IO.Path;

namespace Fork.View.Xaml2.Pages.Settings
{
    public partial class VanillaSettingsPage : Page, ISettingsPage
    {
        public SettingsFile SettingsFile { get; set; }
        public string FileName => Path.GetFileNameWithoutExtension(SettingsFile.FileInfo.FullName);
        public string FileExtension => Path.GetExtension(SettingsFile.FileInfo.FullName);
        
        private SettingsViewModel viewModel;
        private ServerViewModel serverViewModel;
        
        public VanillaSettingsPage(SettingsViewModel viewModel, SettingsFile settingsFile)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.SettingsFile = settingsFile;
            serverViewModel = viewModel.EntityViewModel as ServerViewModel;
            DataContext = serverViewModel;
        }

        public void SaveSettings()
        {
            serverViewModel.SaveProperties();
        }
        
        private void JavaPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog{Multiselect = false, Filter = "Java executable|java.exe", Title = "Select a java.exe"};
            if (!ServerJavaPath.Text.Equals("java.exe") && new DirectoryInfo(ServerJavaPath.Text.Replace(@"\java.exe","")).Exists)
            {
                ofd.InitialDirectory = ServerJavaPath.Text.Replace(@"\java.exe","");
            }
            else
            {
                ofd.InitialDirectory = @"C:\Program Files";
            }
                
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                ServerJavaPath.Text = ofd.FileName;
            }
        }
        
        private void DefaultJavaDirReset_Click(object sender, RoutedEventArgs e)
        {
            ServerJavaPath.Text = "java.exe";
        }
    }
}
