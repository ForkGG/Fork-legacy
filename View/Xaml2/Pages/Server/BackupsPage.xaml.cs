using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fork.Logic.Manager;
using Fork.Logic.Model.BackupModels;
using Fork.ViewModel;

namespace Fork.View.Xaml2.Pages.Server;

public partial class BackupsPage : Page
{
    private Backup pendingRestoreBackup;
    private readonly ServerViewModel viewModel;

    public BackupsPage(ServerViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = this.viewModel;
        KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Escape && RestoreDialogOverlay.Visibility == Visibility.Visible)
            {
                RestoreDialogCancel_Click(sender, e);
            }
        };
    }

    private void OpenExplorer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        string path = BackupManager.Instance.GetBackupPath(viewModel.Server);
        Directory.CreateDirectory(path);
        Process.Start("explorer.exe", "-p, " + path);
    }

    private async void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        await BackupManager.Instance.CreateBackupAsync(createBackupName.Text, viewModel);
    }

    private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: Backup backup } || viewModel.RestoreInProgress)
        {
            return;
        }

        string serverPath = Path.Combine(App.ServerPath, viewModel.Server.Name);
        if (Directory.Exists(serverPath) && Directory.EnumerateFileSystemEntries(serverPath).Any())
        {
            pendingRestoreBackup = backup;
            RestoreDialogOverlay.Visibility = Visibility.Visible;
            return;
        }

        await BackupManager.Instance.RestoreBackupAsync(backup, viewModel, false);
    }

    private async void RestoreDialogKeep_Click(object sender, RoutedEventArgs e)
    {
        await FinishRestoreDialog(false);
    }

    private async void RestoreDialogDelete_Click(object sender, RoutedEventArgs e)
    {
        await FinishRestoreDialog(true);
    }

    private void RestoreDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        pendingRestoreBackup = null;
        RestoreDialogOverlay.Visibility = Visibility.Collapsed;
    }

    private async Task FinishRestoreDialog(bool deleteExisting)
    {
        Backup backup = pendingRestoreBackup;
        pendingRestoreBackup = null;
        RestoreDialogOverlay.Visibility = Visibility.Collapsed;
        if (backup != null)
        {
            await BackupManager.Instance.RestoreBackupAsync(backup, viewModel, deleteExisting);
        }
    }

    private async void DeleteBackup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: Backup backup })
        {
            return;
        }

        await BackupManager.Instance.DeleteBackupAsync(backup, viewModel);
    }
}
