using System.Collections.Generic;
using System.IO;
using Fork.logic.model.PluginModels;
using Fork.ViewModel;
using Newtonsoft.Json;

namespace Fork.Logic.Persistence
{
    public class InstalledPluginSerializer
    {
        private static InstalledPluginSerializer instance;
        private static readonly object writeLock = new();

        private InstalledPluginSerializer()
        {
        }

        public static InstalledPluginSerializer Instance => instance ??= new InstalledPluginSerializer();

        public void StoreInstalledPlugins(List<InstalledPlugin> plugins, EntityViewModel viewModel)
        {
            string json = JsonConvert.SerializeObject(plugins, Formatting.Indented);
            DirectoryInfo directoryInfo =
                new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Entity.Name, "plugins"));
            if (!directoryInfo.Exists) directoryInfo.Create();
            FileInfo entitiesFile = new FileInfo(Path.Combine(directoryInfo.FullName, "plugins.json"));
            lock (writeLock)
            {
                using (FileStream fs = entitiesFile.Create())
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(json);
                }
            }
        }

        public List<InstalledPlugin> LoadInstalledPlugins(EntityViewModel viewModel)
        {
            DirectoryInfo persistenceDir =
                new DirectoryInfo(Path.Combine(App.ServerPath, viewModel.Entity.Name, "plugins"));
            if (!persistenceDir.Exists) persistenceDir.Create();
            List<InstalledPlugin> result = new List<InstalledPlugin>();
            FileInfo pluginsFile = new FileInfo(Path.Combine(persistenceDir.FullName, "plugins.json"));
            if (!pluginsFile.Exists) return result;

            result = JsonConvert.DeserializeObject<List<InstalledPlugin>>(File.ReadAllText(pluginsFile.FullName));

            //Overwrite json if any changes appeared while deserializing (Unset value set etc.)
            //StoreInstalledPlugins(result);
            return result;
        }
    }
}