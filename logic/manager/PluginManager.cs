using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
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

namespace Fork.Logic.Manager
{
    public class PluginManager
    {
        private static PluginManager instance;
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

        private PluginManager()
        {
        }

        public async Task<bool> DownloadPluginAsync(InstalledPlugin plugin, EntityViewModel viewModel)
        {
            return await DownloadPlugin(plugin, viewModel);
        }

        public async Task<bool> InstallPluginAsync(Plugin plugin, PluginViewModel pluginViewModel)
        {
            Task<bool> t = new Task<bool>(() => InstallPlugin(plugin, pluginViewModel));
            t.Start();
            bool r = await t;
            return r;
        }
        
        public async Task<bool> DeletePluginAsync(InstalledPlugin plugin, PluginViewModel pluginViewModel)
        {
            Task<bool> t = new Task<bool>(() => DeletePlugin(plugin, pluginViewModel));
            t.Start();
            bool r = await t;
            return r;
        }
        
        public async Task<bool> DisablePluginAsync(InstalledPlugin plugin, PluginViewModel pluginViewModel)
        {
            Task<bool> t = new Task<bool>(() => DisablePlugin(plugin, pluginViewModel));
            t.Start();
            bool r = await t;
            return r;
        }
        public async Task<bool> EnablePluginAsync(InstalledPlugin plugin, PluginViewModel pluginViewModel)
        {
            Task<bool> t = new Task<bool>(() => EnablePlugin(plugin, pluginViewModel));
            t.Start();
            bool r = await t;
            return r;
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
            
            APIController apiController = new APIController();
            try
            {
                var result = await apiController.DownloadPluginAsync(plugin,
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
            foreach (InstalledPlugin iPlugin in pluginViewModel.InstalledPlugins)
            {
                if (iPlugin.Plugin == null)
                {
                    continue;
                }
                if (iPlugin.Plugin.id == plugin.id)
                {
                    Console.WriteLine("Error installing plugin: Plugin "+plugin.name+" is already installed.");
                    return false;
                }
            }
            
            PluginWebRequester webRequester = new PluginWebRequester();
            InstalledPlugin installedPlugin = new InstalledPlugin
            {
                Name = StringUtils.BeautifyPluginName(plugin.name),
                IsSpigetPlugin = true,
                SpigetId = plugin.id,
                InstalledVersion = webRequester.RequestLatestVersion(plugin.id)
            };
            Application.Current.Dispatcher?.Invoke(() => pluginViewModel.InstalledPlugins.Add(installedPlugin));
            installedPlugin.AfterInit(new StreamingContext());
            //TODO attach something to update event
            Console.WriteLine("Installed Plugin "+installedPlugin.Name);

            return DownloadPlugin(installedPlugin, pluginViewModel.EntityViewModel).Result;
        }

        private bool DeletePlugin(InstalledPlugin plugin, PluginViewModel viewModel)
        {
            try
            {
                string folder = plugin.IsEnabled ? "plugins" : "plugins_disabled";
                FileInfo jarFile = new FileInfo(Path.Combine(App.ServerPath,
                    viewModel.EntityViewModel.Name, folder, StringUtils.PluginNameToJarName(plugin.Name)+".jar"));
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
                viewModel.CheckForDeletedPlugin(plugin.Plugin);
                Console.WriteLine("Deleted Plugin "+plugin.Name);
                return true;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("Error while deleting Plugin "+plugin.Name);
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
                FileInfo jarFile = new FileInfo(Path.Combine(App.ServerPath,
                    viewModel.EntityViewModel.Name, "plugins", StringUtils.PluginNameToJarName(plugin.Name)+".jar"));
                if (!jarFile.Exists)
                {
                    ErrorLogger.Append(new ArgumentException(
                        ".jar for plugin " + plugin.Name + " was not found. Can't disable it. Removing it from the list..."));
                    Application.Current.Dispatcher?.Invoke(() => viewModel.InstalledPlugins.Remove(plugin));
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ServerPath, 
                        viewModel.EntityViewModel.Name, "plugins_disabled"));
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    jarFile.MoveTo(Path.Combine(directoryInfo.FullName,jarFile.Name), true);
                }

                Console.WriteLine("Disabled Plugin "+plugin.Name);
                viewModel.DisablePlugin(plugin);
                return true;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("Error while disabling Plugin "+plugin.Name);
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
                FileInfo jarFile = new FileInfo(Path.Combine(App.ServerPath,
                    viewModel.EntityViewModel.Name, "plugins_disabled", StringUtils.PluginNameToJarName(plugin.Name)+".jar"));
                if (!jarFile.Exists)
                {
                    ErrorLogger.Append(new ArgumentException(
                        ".jar for plugin " + plugin.Name + " was not found. Can't enable it. Removing it from the list..."));
                    Application.Current.Dispatcher?.Invoke(() => viewModel.InstalledPlugins.Remove(plugin));
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(App.ServerPath, 
                        viewModel.EntityViewModel.Name, "plugins"));
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    jarFile.MoveTo(Path.Combine(directoryInfo.FullName,jarFile.Name), true);
                }

                Console.WriteLine("Enabled Plugin "+plugin.Name);
                viewModel.EnablePlugin(plugin);
                return true;
            }
            catch (Exception e)
            {
                ErrorLogger.Append(e);
                Console.WriteLine("Error while enabling Plugin "+plugin.Name);
                return false;
            }
        }
    }
}