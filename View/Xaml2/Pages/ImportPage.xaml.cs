using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Fork.Logic.ImportLogic;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.ViewModel;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;

namespace Fork.View.Xaml2.Pages
{
    /// <summary>
    ///     Interaktionslogik für ImportServerPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        private string lastPath;
        private readonly ImportViewModel viewModel;

        public ImportPage()
        {
            InitializeComponent();
            viewModel = new ImportViewModel();
            DataContext = viewModel;
        }

        private void ServerTypeVanilla_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty,
                new Binding {Source = viewModel.VanillaServerVersions});
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypePaper_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty,
                new Binding {Source = viewModel.PaperVersions});
            versionComboBox.SelectedIndex = 0;
        }

        private void ServerTypeSpigot_Click(object sender, RoutedEventArgs e)
        {
            versionComboBox.SetBinding(ItemsControl.ItemsSourceProperty,
                new Binding {Source = viewModel.SpigotServerVersions});
            versionComboBox.SelectedIndex = 0;
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            ServerVersion selectedVersion = (ServerVersion) versionComboBox.SelectedValue;
            //TODO check if inputs are valid / server not existing

            if (lastPath == null) throw new Exception("Import Button should not be clickable, as path is not valid");

            DirectoryInfo oldDir = new DirectoryInfo(lastPath);
            string serverName = oldDir.Name;
            ServerValidationInfo validationInfo = DirectoryValidator.ValidateServerDirectory(oldDir);

            bool createServerSuccess =
                await ServerManager.Instance.ImportServerAsync(selectedVersion, validationInfo, oldDir.FullName,
                    serverName);

            //TODO Do something if creating fails
        }

        private void ServerDirPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (lastPath != null) fbd.SelectedPath = lastPath;

            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                serverFolderPathText.Text = fbd.SelectedPath;
                lastPath = fbd.SelectedPath;
                ServerValidationInfo valInfo = null;
                try
                {
                    valInfo = DirectoryValidator.ValidateServerDirectory(new DirectoryInfo(fbd.SelectedPath));
                }
                catch (Exception ex)
                {
                    ErrorLogger.Append(ex);
                }

                if (valInfo == null || !valInfo.IsValid)
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