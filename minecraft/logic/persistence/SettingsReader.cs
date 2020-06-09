using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using fork.Logic.Manager;
using fork.Logic.Model;
using fork.Logic.Model.Settings;
using fork.Logic.Persistence.PersistencePO;
using fork.ViewModel;

namespace fork.Logic.Persistence
{
    public class SettingsReader
    {
        private FileSystemWatcher fileWatcher;
        private EntityViewModel viewModel;

        public SettingsReader(EntityViewModel viewModel)
        {
            viewModel.UpdateSettingsFiles(GetSettingsFiles(viewModel), true);
            WatchFileChanges(new DirectoryInfo(Path.Combine(App.ApplicationPath, viewModel.Entity.Name)));
        }

        public void Dispose()
        {
            fileWatcher.Changed -= OnFilesChanged;
            fileWatcher.Dispose();
        }
        
        private List<SettingsFile> GetSettingsFiles(EntityViewModel viewModel)
        {
            this.viewModel = viewModel;
            string path = Path.Combine(App.ApplicationPath, viewModel.Entity.Name);
            DirectoryInfo entitiyDir = new DirectoryInfo(path);
            
            List<SettingsFile> settingsFiles = new List<SettingsFile>();
            foreach (FileInfo fileInfo in entitiyDir.EnumerateFiles())
            {
                if (fileInfo.Name.EndsWith(".properties") || fileInfo.Name.EndsWith(".yml"))
                {
                    SettingsFile settingsFile = new SettingsFile(fileInfo);
                    settingsFiles.Add(settingsFile);
                }
            }

            return settingsFiles;
        }

        private void WatchFileChanges(DirectoryInfo directoryInfo)
        {
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = directoryInfo.FullName;
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Filter = "*.*";
            fileWatcher.Changed += OnFilesChanged;
            fileWatcher.EnableRaisingEvents = true;
        }

        private void OnFilesChanged(object source, FileSystemEventArgs e)
        {
            viewModel.UpdateSettingsFiles(GetSettingsFiles(viewModel));
        }
    }
}