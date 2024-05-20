using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fork.Logic.Logging;
using Fork.Logic.Model.Settings;
using Fork.ViewModel;

namespace Fork.Logic.Persistence;

public class SettingsReader
{
    private FileSystemWatcher fileWatcher;
    private EntityViewModel viewModel;

    public SettingsReader(EntityViewModel viewModel)
    {
        viewModel.InitializeSettingsFiles(GetSettingsFiles(viewModel));
        SetupFileWatcher(new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Name)));
        viewModel.EntityPathChangedEvent += HandleEntityPathChangedEvent;
    }

    private void HandleEntityPathChangedEvent(object sender, EntityViewModel.EntityPathChangedEventArgs e)
    {
        viewModel.InitializeSettingsFiles(GetSettingsFiles(viewModel));
        fileWatcher.Dispose();
        SetupFileWatcher(new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Name)));
    }

    public void Dispose()
    {
        if (fileWatcher != null)
        {
            fileWatcher.Changed -= OnFilesChanged;
            fileWatcher.Dispose();
        }
    }

    private List<SettingsFile> GetSettingsFiles(EntityViewModel viewModel)
    {
        this.viewModel = viewModel;
        string path = Path.Combine(App.ServerPath, viewModel.Name);
        DirectoryInfo entitiyDir = new(path);
        List<SettingsFile> settingsFiles = new();
        if (!entitiyDir.Exists)
        {
            Console.WriteLine("Directory for entity " + viewModel.Entity.Name + " does not exist.\nPath: \"" +
                              entitiyDir.FullName + "\"");
            return settingsFiles;
        }

        try
        {
            foreach (string fileName in Directory.GetFiles(entitiyDir.FullName, "*.properties",
                         SearchOption.TopDirectoryOnly))
            {
                FileInfo fileInfo = new(Path.Combine(entitiyDir.FullName, fileName));
                SettingsFile settingsFile = new(fileInfo);
                settingsFiles.Add(settingsFile);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error looking for properties files in directory: " + path);
            ErrorLogger.Append(e);
        }

        try
        {
            foreach (string fileName in Directory.GetFiles(entitiyDir.FullName, "*.yml", SearchOption.TopDirectoryOnly))
            {
                FileInfo fileInfo = new(Path.Combine(entitiyDir.FullName, fileName));
                SettingsFile settingsFile = new(fileInfo);
                settingsFiles.Add(settingsFile);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error looking for yml files in directory: " + path);
            ErrorLogger.Append(e);
        }

        return settingsFiles;
    }

    private async Task SetupFileWatcher(DirectoryInfo directoryInfo)
    {
        fileWatcher = new FileSystemWatcher();
        fileWatcher.Path = directoryInfo.FullName;
        fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
        fileWatcher.Filters.Add("*.yml");
        fileWatcher.Filters.Add("*.properties");
        fileWatcher.Changed += OnFilesChanged;
        fileWatcher.EnableRaisingEvents = true;
    }

    private void OnFilesChanged(object source, FileSystemEventArgs e)
    {
        viewModel.UpdateSettingsFiles(new List<string> { e.FullPath });
    }
}