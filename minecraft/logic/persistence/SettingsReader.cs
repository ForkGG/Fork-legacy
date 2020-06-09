using System.Collections.Generic;
using System.IO;
using fork.Logic.Model.Settings;
using fork.ViewModel;

namespace fork.Logic.Persistence
{
    public class SettingsReader
    {
        public static List<SettingsFile> GetSettingsFiles(EntityViewModel viewModel)
        {
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
    }
}