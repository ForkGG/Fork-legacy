using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using nihilus.Logic.ImportLogic;
using nihilus.ViewModel;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using ComboBox = System.Windows.Controls.ComboBox;

namespace nihilus.View.Xaml2.Pages
{
    /// <summary>
    /// Interaktionslogik für ImportServerPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        private ImportViewModel viewModel;
        private string lastPath;
        
        public ImportPage()
        {
            InitializeComponent();
            viewModel = new ImportViewModel();
            DataContext = viewModel;
        }
        
        private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.VanillaServerVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.PaperVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypeSpigot_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = viewModel.SpigotServerVersions });
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerDirPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (lastPath != null)
            {
                fbd.SelectedPath = lastPath;
            }
            
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                serverFolderPathText.Text = fbd.SelectedPath;
                lastPath = fbd.SelectedPath;
                ServerValidationInfo valInfo = DirectoryValidator.ValidateServerDirectory(new DirectoryInfo(fbd.SelectedPath));
                if (!valInfo.IsValid)
                {
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("buttonBgrRed");
                    ImportConfirmButton.IsEnabled = false;
                }
                else
                {
                    serverPathBgr.Background = (Brush) Application.Current.FindResource("tabSelected");
                    ImportConfirmButton.IsEnabled = true;
                }
            }
        }
    }
}
