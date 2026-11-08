using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Fork.Logic.CustomConsole;
using Fork.Logic.ImportLogic;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.Logic.Model.BackupModels;
using Fork.Logic.Model.ServerConsole;
using Fork.ViewModel;

namespace Fork.Logic.Manager;

public sealed class BackupManager
{
    private static BackupManager instance;

    private BackupManager()
    {
    }

    public static BackupManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BackupManager();
            }

            return instance;
        }
    }

    public string GetBackupPath(Server server)
    {
        return Path.Combine(App.ApplicationPath, "backups", server.Name);
    }

    public void InitializeBackupsList(ServerViewModel viewModel)
    {
        DirectoryInfo backupDir = new(GetBackupPath(viewModel.Server));
        Application.Current.Dispatcher?.Invoke(() => viewModel.Backups.Clear());
        if (!backupDir.Exists)
        {
            return;
        }

        foreach (FileInfo file in backupDir.EnumerateFiles("*.zip").OrderByDescending(f => f.CreationTime))
        {
            Backup backup = new(file);
            Application.Current.Dispatcher?.Invoke(() => viewModel.Backups.Add(backup));
        }
    }

    public void RenameBackupFolder(string oldName, string newName)
    {
        string oldPath = Path.Combine(App.ApplicationPath, "backups", oldName);
        string newPath = Path.Combine(App.ApplicationPath, "backups", newName);
        if (Directory.Exists(oldPath) && !Directory.Exists(newPath))
        {
            Directory.Move(oldPath, newPath);
        }
    }

    private bool CreateBackup(string name, ServerViewModel viewModel)
    {
        string zipPath = null;
        try
        {
            if (viewModel.RestoreInProgress)
            {
                ConsoleWriter.Write(new ConsoleMessage(
                    "Can not create a backup while a restore is in progress!",
                    ConsoleMessage.MessageLevel.ERROR), viewModel);
                return false;
            }

            DirectoryInfo serverDir = new(Path.Combine(App.ServerPath, viewModel.Server.Name));
            if (!serverDir.Exists)
            {
                ConsoleWriter.Write("Could not create backup! Server directory does not exist", viewModel);
                return false;
            }

            if (viewModel.CurrentStatus == ServerStatus.RUNNING)
            {
                ConsoleWriter.Write("Server is running. Saving world data before creating the backup...",
                    viewModel);
                ApplicationManager.Instance.ActiveEntities[viewModel.Server].StandardInput
                    .WriteLineAsync("save-all");
                //Give the server a moment to flush its data to disk
                Thread.Sleep(2000);
            }

            string backupDirPath = GetBackupPath(viewModel.Server);
            Directory.CreateDirectory(backupDirPath);
            string backupName = GetValidBackupName(name, backupDirPath);
            zipPath = Path.Combine(backupDirPath, backupName + ".zip");

            ConsoleWriter.Write("Creating backup " + backupName + "...", viewModel);
            viewModel.BackupInProgress = true;
            viewModel.BackupProgress = 0;
            CreateZip(serverDir, zipPath, viewModel);

            FileInfo backupFile = new(zipPath);
            ConsoleWriter.Write(new ConsoleMessage(
                "Successfully created backup " + backupName + " (" + new Backup(backupFile).Size + ")",
                ConsoleMessage.MessageLevel.SUCCESS), viewModel);
            InitializeBackupsList(viewModel);
            return true;
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            ConsoleWriter.Write(new ConsoleMessage(
                "Error while creating backup! See error log for details (" + e.Message + ")",
                ConsoleMessage.MessageLevel.ERROR), viewModel);
            if (zipPath != null && File.Exists(zipPath))
            {
                try
                {
                    File.Delete(zipPath);
                }
                catch (Exception)
                {
                    //The partially created backup could not be deleted
                }
            }

            return false;
        }
        finally
        {
            viewModel.BackupInProgress = false;
        }
    }

    private void CreateZip(DirectoryInfo serverDir, string zipPath, ServerViewModel viewModel)
    {
        List<FileInfo> files = serverDir.GetFiles("*", SearchOption.AllDirectories).ToList();
        int filesTotal = files.Count;
        int filesProcessed = 0;
        using (FileStream zipStream = new(zipPath, FileMode.Create))
        {
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);
            foreach (DirectoryInfo directory in serverDir.GetDirectories("*", SearchOption.AllDirectories))
            {
                string relativeDir = Path.GetRelativePath(serverDir.FullName, directory.FullName);
                archive.CreateEntry(relativeDir.Replace('\\', '/') + "/");
            }

            foreach (FileInfo file in files)
            {
                string relativePath = Path.GetRelativePath(serverDir.FullName, file.FullName);
                archive.CreateEntryFromFile(file.FullName, relativePath.Replace('\\', '/'),
                    CompressionLevel.Optimal);
                filesProcessed++;
                viewModel.BackupProgress = filesTotal == 0
                    ? 100
                    : filesProcessed / (double)filesTotal * 100;
            }
        }
    }

    private bool RestoreBackup(Backup backup, ServerViewModel viewModel, bool deleteExisting)
    {
        if (viewModel.CurrentStatus != ServerStatus.STOPPED)
        {
            ConsoleWriter.Write(new ConsoleMessage(
                "Can not restore a backup while the server is running! Stop the server first.",
                ConsoleMessage.MessageLevel.ERROR), viewModel);
            return false;
        }

        if (viewModel.BackupInProgress || viewModel.RestoreInProgress)
        {
            ConsoleWriter.Write(new ConsoleMessage(
                "Can not restore a backup while another backup operation is in progress!",
                ConsoleMessage.MessageLevel.ERROR), viewModel);
            return false;
        }

        string tempPath = null;
        try
        {
            DirectoryInfo serverDir = new(Path.Combine(App.ServerPath, viewModel.Server.Name));
            if (!serverDir.Exists)
            {
                ConsoleWriter.Write("Could not restore backup! Server directory does not exist", viewModel);
                return false;
            }

            ConsoleWriter.Write("Restoring backup " + backup.Name + "...", viewModel);
            viewModel.RestoreInProgress = true;
            viewModel.RestoreProgress = 0;

            //Extract the backup to a temporary directory first, so the server files stay
            //intact if the extraction fails
            tempPath = Path.Combine(App.ApplicationPath, "tmp",
                "restore-" + Guid.NewGuid().ToString("D").Replace("-", ""));
            ExtractZip(backup, tempPath, viewModel);

            if (deleteExisting)
            {
                ConsoleWriter.Write("Deleting existing server files...", viewModel);
                foreach (FileInfo file in serverDir.GetFiles()) file.Delete();
                foreach (DirectoryInfo dir in serverDir.GetDirectories()) dir.Delete(true);
            }

            FileImporter fileImporter = new();
            fileImporter.CopyProgressChanged += (o, args) =>
                viewModel.RestoreProgress = args.FilesToCopy == 0
                    ? 100
                    : 50 + args.FilesCopied / (double)args.FilesToCopy * 50;
            fileImporter.DirectoryCopy(tempPath, serverDir.FullName, true);

            viewModel.InitializeWorldsList();
            ConsoleWriter.Write(new ConsoleMessage(
                "Successfully restored backup " + backup.Name, ConsoleMessage.MessageLevel.SUCCESS), viewModel);
            return true;
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            ConsoleWriter.Write(new ConsoleMessage(
                "Error while restoring backup! See error log for details (" + e.Message + ")",
                ConsoleMessage.MessageLevel.ERROR), viewModel);
            return false;
        }
        finally
        {
            viewModel.RestoreInProgress = false;
            if (tempPath != null && Directory.Exists(tempPath))
            {
                try
                {
                    Directory.Delete(tempPath, true);
                }
                catch (Exception)
                {
                    //The temporary restore directory could not be deleted
                }
            }
        }
    }

    private void ExtractZip(Backup backup, string tempPath, ServerViewModel viewModel)
    {
        using ZipArchive archive = ZipFile.OpenRead(backup.File.FullName);
        int entriesTotal = archive.Entries.Count;
        int entriesProcessed = 0;
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string destinationPath = Path.Combine(tempPath, entry.FullName);
            if (entry.FullName.EndsWith("/"))
            {
                Directory.CreateDirectory(destinationPath);
            }
            else
            {
                string parentDir = Path.GetDirectoryName(destinationPath);
                if (parentDir != null)
                {
                    Directory.CreateDirectory(parentDir);
                }

                entry.ExtractToFile(destinationPath, true);
            }

            entriesProcessed++;
            viewModel.RestoreProgress = entriesTotal == 0
                ? 50
                : entriesProcessed / (double)entriesTotal * 50;
        }
    }

    private bool DeleteBackup(Backup backup, ServerViewModel viewModel)
    {
        if (viewModel.RestoreInProgress)
        {
            ConsoleWriter.Write(new ConsoleMessage(
                "Can not delete a backup while a restore is in progress!",
                ConsoleMessage.MessageLevel.ERROR), viewModel);
            return false;
        }

        try
        {
            backup.File.Refresh();
            if (backup.File.Exists)
            {
                backup.File.Delete();
            }

            ConsoleWriter.Write("Deleted backup " + backup.Name, viewModel);
            InitializeBackupsList(viewModel);
            return true;
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            ConsoleWriter.Write(new ConsoleMessage(
                "Error while deleting backup! See error log for details (" + e.Message + ")",
                ConsoleMessage.MessageLevel.ERROR), viewModel);
            return false;
        }
    }

    private string GetValidBackupName(string name, string backupDirPath)
    {
        DateTime now = DateTime.Now;
        string timeStamp = now.Day + "-" + now.Month + "-" + now.Year + "_" +
                           now.Hour + "-" + now.Minute + "-" + now.Second;
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Backup";
        }

        string result = name + "_" + timeStamp;
        string resultRaw = result;
        int i = 1;
        while (File.Exists(Path.Combine(backupDirPath, result + ".zip")))
        {
            result = resultRaw + "_" + i;
            i++;
        }

        return result;
    }

    #region Backup Managment Methods

    public async Task<bool> CreateBackupAsync(string name, ServerViewModel viewModel)
    {
        Task<bool> t = new(() => CreateBackup(name, viewModel));
        t.Start();
        bool result = await t;
        return result;
    }

    public async Task<bool> RestoreBackupAsync(Backup backup, ServerViewModel viewModel, bool deleteExisting)
    {
        Task<bool> t = new(() => RestoreBackup(backup, viewModel, deleteExisting));
        t.Start();
        bool result = await t;
        return result;
    }

    public async Task<bool> DeleteBackupAsync(Backup backup, ServerViewModel viewModel)
    {
        Task<bool> t = new(() => DeleteBackup(backup, viewModel));
        t.Start();
        bool result = await t;
        return result;
    }

    #endregion
}
