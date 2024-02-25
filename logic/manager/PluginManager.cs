using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Fork.Logic.Controller;
using Fork.Logic.Logging;
using Fork.logic.model.PluginModels;
using Fork.Logic.Model.PluginModels;
using Fork.Logic.Persistence;
using Fork.Logic.Utils;
using Fork.Logic.WebRequesters;
using Fork.ViewModel;

namespace Fork.Logic.Manager;

public class PluginManager
{
    private static PluginManager instance;

    private PluginManager()
    {
    }

    public static PluginManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PluginManager();
            }

            return instance;
        }
    }

    public async Task<bool> DownloadPluginAsync(InstalledPlugin plugin, EntityViewModel viewModel)
    {
        return await DownloadPlugin(plugin, viewModel);
    }

    public async Task<bool> InstallPluginAsync(Plugin plugin, PluginViewModel pluginViewModel)
    {
        Task<bool> t = new(() => InstallPlugin(plugin, pluginViewModel));
        t.Start();
        bool r = await t;
        return r;
    }

    public async Task<bool> DeletePluginAsync(InstalledPlugin plugin, PluginViewModel pluginViewModel)
    {
        Task<bool> t = new(() => DeletePlugin(plugin, pluginViewModel));
        t.Start();
        bool r = await t;
        return r;
    }

    public async Task<bool> DisablePluginAsync(InstalledPlugin plugin, PluginViewModel pluginViewModel)
    {
        Task<bool> t = new(() => DisablePlugin(plugin, pluginViewModel));
        t.Start();
        bool r = await t;
        return r;
    }

    public async Task<bool> EnablePluginAsync(InstalledPlugin plugin, PluginViewModel pluginViewModel)
    {
        Task<bool> t = new(() => EnablePlugin(plugin, pluginViewModel));
        t.Start();
        bool r = await t;
        return r;
    }

    public List<InstalledPlugin> LoadInstalledPlugins(Collection<InstalledPlugin> alreadyTrackedPlugins,
        EntityViewModel viewModel)
    {
        List<FileInfo> pluginFiles =
            InstalledPluginSerializer.Instance.ReadPluginJarsFromDirectory(viewModel);
        List<InstalledPlugin> result = new();
        foreach (FileInfo pluginFile in pluginFiles)
            if (!alreadyTrackedPlugins.Any(plugin => plugin.Name.Equals(pluginFile.Name.Split(".")[0])))
            {
                result.Add(new InstalledPlugin
                    { IsDownloaded = true, IsEnabled = true, IsSpigetPlugin = false, Name = pluginFile.Name });
            }

        return result;
    }


    private async Task<bool> DownloadPlugin(InstalledPlugin plugin, EntityViewModel viewModel)
    {
        if (plugin.IsDownloaded)
        {
            return true;
        }

        if (!plugin.IsSpigetPlugin || plugin.Plugin.file.type.Equals("external"))
        {
            return false;
        }

        APIController apiController = new();
        try
        {
            FileInfo result = await apiController.DownloadPluginAsync(plugin,
                Path.Combine(App.ServerPath, viewModel.Entity.Name, "plugins",
                    StringUtils.PluginNameToJarName(plugin.Name) + ".jar"));
            if (result == null)
            {
                return false;
            }
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            return false;
        }


        plugin.IsDownloaded = true;
        return true;
    }

    private bool InstallPlugin(Plugin plugin, PluginViewModel pluginViewModel)
    {
        Collection<InstalledPlugin> removeTarget = new();
        foreach (InstalledPlugin iPlugin in pluginViewModel.InstalledPlugins)
        {
            if (iPlugin.Plugin == null)
            {
                if (iPlugin.Name.Equals(plugin.name))
                {
                    Console.WriteLine($"{iPlugin.Name} marked for deletion");
                    removeTarget.Add(iPlugin);

                    pluginViewModel.DisablePlugin(iPlugin);
                }

                continue;
            }

            if (iPlugin.Plugin.id == plugin.id)
            {
                Console.WriteLine("Error installing plugin: Plugin " + plugin.name + " is already installed.");
                return false;
            }
        }

        foreach (InstalledPlugin iPlugin in removeTarget)
        {
            Console.WriteLine($"Removing locally installed plugin {iPlugin.Name} because installing remotely");
            DeletePlugin(iPlugin, pluginViewModel);
        }

        PluginWebRequester webRequester = new();
        InstalledPlugin installedPlugin = new()
        {
            Name = StringUtils.BeautifyPluginName(plugin.name),
            IsSpigetPlugin = true,
            SpigetId = plugin.id,
            InstalledVersion = webRequester.RequestLatestVersion(plugin.id)
        };
        Application.Current.Dispatcher?.Invoke(() => pluginViewModel.InstalledPlugins.Add(installedPlugin));
        installedPlugin.AfterInit(new StreamingContext());
        //TODO attach something to update event
        Console.WriteLine("Installed Plugin " + installedPlugin.Name);

        return DownloadPlugin(installedPlugin, pluginViewModel.EntityViewModel).Result;
    }

    private bool DeletePlugin(InstalledPlugin plugin, PluginViewModel viewModel)
    {
        try
        {
            string folder = plugin.IsEnabled ? "plugins" : "plugins_disabled";
            FileInfo jarFile = new(Path.Combine(App.ServerPath,
                viewModel.EntityViewModel.Name, folder, StringUtils.PluginNameToJarName(plugin.Name) + ".jar"));
            if (!jarFile.Exists)
            {
                jarFile = new FileInfo(Path.Combine(App.ServerPath, viewModel.EntityViewModel.Name, folder,
                    $"{plugin.Name}"));
            }

            if (!jarFile.Exists)
            {
                ErrorLogger.Append(new ArgumentException(
                    ".jar for plugin " + plugin.Name + " was not found. Removing it from the list..."));
            }
            else
            {
                jarFile.Delete();
            }

            Application.Current.Dispatcher?.Invoke(() => viewModel.InstalledPlugins.Remove(plugin));
            //Check if plugin is in loaded list currently (in that case change it to downlaodable)
            if (plugin.LocalPlugin == null)
            {
                viewModel.CheckForDeletedPlugin(plugin.Plugin);
            }
            else
            {
                viewModel.InstalledPlugins.Remove(plugin);
                InstalledPluginSerializer.Instance.StoreInstalledPlugins(viewModel.InstalledPlugins.ToList(),
                    viewModel.EntityViewModel);
            }

            Console.WriteLine("Deleted Plugin " + plugin.Name);
            return true;
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            Console.WriteLine("Error while deleting Plugin " + plugin.Name);
            return false;
        }
    }

    private bool DisablePlugin(InstalledPlugin plugin, PluginViewModel viewModel)
    {
        try
        {
            if (!plugin.IsEnabled)
            {
                return true;
            }

            FileInfo jarFile = new(Path.Combine(App.ServerPath,
                viewModel.EntityViewModel.Name, "plugins", StringUtils.PluginNameToJarName(plugin.Name) + ".jar"));
            if (!jarFile.Exists)
            {
                ErrorLogger.Append(new ArgumentException(
                    ".jar for plugin " + plugin.Name +
                    " was not found. Can't disable it. Removing it from the list..."));
                Application.Current.Dispatcher?.Invoke(() => viewModel.InstalledPlugins.Remove(plugin));
            }
            else
            {
                DirectoryInfo directoryInfo = new(Path.Combine(App.ServerPath,
                    viewModel.EntityViewModel.Name, "plugins_disabled"));
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                jarFile.MoveTo(Path.Combine(directoryInfo.FullName, jarFile.Name), true);
            }

            Console.WriteLine("Disabled Plugin " + plugin.Name);
            viewModel.DisablePlugin(plugin);
            return true;
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            Console.WriteLine("Error while disabling Plugin " + plugin.Name);
            return false;
        }
    }

    private bool EnablePlugin(InstalledPlugin plugin, PluginViewModel viewModel)
    {
        try
        {
            if (plugin.IsEnabled)
            {
                return true;
            }

            FileInfo jarFile = new(Path.Combine(App.ServerPath,
                viewModel.EntityViewModel.Name, "plugins_disabled",
                StringUtils.PluginNameToJarName(plugin.Name) + ".jar"));
            if (!jarFile.Exists)
            {
                ErrorLogger.Append(new ArgumentException(
                    ".jar for plugin " + plugin.Name +
                    " was not found. Can't enable it. Removing it from the list..."));
                Application.Current.Dispatcher?.Invoke(() => viewModel.InstalledPlugins.Remove(plugin));
            }
            else
            {
                DirectoryInfo directoryInfo = new(Path.Combine(App.ServerPath,
                    viewModel.EntityViewModel.Name, "plugins"));
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                jarFile.MoveTo(Path.Combine(directoryInfo.FullName, jarFile.Name), true);
            }

            Console.WriteLine("Enabled Plugin " + plugin.Name);
            viewModel.EnablePlugin(plugin);
            return true;
        }
        catch (Exception e)
        {
            ErrorLogger.Append(e);
            Console.WriteLine("Error while enabling Plugin " + plugin.Name);
            return false;
        }
    }
}