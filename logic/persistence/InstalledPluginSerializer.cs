using System.Collections.Generic;
using System.IO;
using Fork.logic.model.PluginModels;
using Fork.ViewModel;
using Newtonsoft.Json;

namespace Fork.Logic.Persistence;

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
        DirectoryInfo directoryInfo = new(Path.Combine(App.ServerPath, viewModel.Entity.Name, "plugins"));
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        FileInfo entitiesFile = new(Path.Combine(directoryInfo.FullName, "plugins.json"));
        lock (writeLock)
        {
            using FileStream fs = entitiesFile.Create();
            using StreamWriter sw = new(fs);
            sw.WriteLine(json);
        }
    }

    public List<InstalledPlugin> LoadInstalledPlugins(EntityViewModel viewModel)
    {
        DirectoryInfo persistenceDir = new(Path.Combine(App.ServerPath, viewModel.Entity.Name, "plugins"));
        if (!persistenceDir.Exists)
        {
            persistenceDir.Create();
        }

        List<InstalledPlugin> result = new();
        FileInfo pluginsFile = new(Path.Combine(persistenceDir.FullName, "plugins.json"));
        if (!pluginsFile.Exists)
        {
            return result;
        }

        result = JsonConvert.DeserializeObject<List<InstalledPlugin>>(File.ReadAllText(pluginsFile.FullName));

        //Overwrite json if any changes appeared while deserializing (Unset value set etc.)
        //StoreInstalledPlugins(result);
        return result;
    }

    public List<FileInfo> ReadPluginJarsFromDirectory(EntityViewModel viewModel)
    {
        DirectoryInfo pluginDir = new(Path.Combine(App.ServerPath, viewModel.Entity.Name, "plugins"));
        if (!pluginDir.Exists)
        {
            return new List<FileInfo>();
        }

        FileInfo[] files = pluginDir.GetFiles();
        List<FileInfo> result = new(files.Length);
        foreach (FileInfo fileInfo in files)
            if (fileInfo.Extension.Equals(".jar"))
            {
                result.Add(fileInfo);
            }

        return result;
    }
}