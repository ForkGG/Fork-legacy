using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.logic.model.PluginModels;
using Fork.Logic.Model.PluginModels;
using Fork.Logic.Persistence;
using Fork.Logic.WebRequesters;

namespace Fork.ViewModel
{
    public class PluginViewModel : BaseViewModel
    {
        private PluginWebRequester pluginWebRequester;
        private int lastPage = 1;
        private bool fullyLoaded = false;
        
        public ObservableCollection<Plugin> Plugins { get; private set; }
        
        public ObservableCollection<InstalledPlugin> InstalledPlugins { get; }
        
        public List<PluginEnums.Sorting> Sortings { get; } = 
            new List<PluginEnums.Sorting>(Enum.GetValues(typeof(PluginEnums.Sorting)).Cast<PluginEnums.Sorting>());
        public PluginEnums.Sorting Sorting { get; set; }
        public List<PluginCategory> Categories { get; private set; }
        public PluginCategory SelectedCategory { get; set; }
        public string SearchQuery { get; set; } = "";
        public bool FullyLoaded => fullyLoaded;
        
        public EntityViewModel EntityViewModel { get; }

        public PluginViewModel(EntityViewModel entityViewModel)
        {
            InstalledPlugins = new ObservableCollection<InstalledPlugin>(
                InstalledPluginSerializer.Instance.LoadInstalledPlugins(entityViewModel));
            foreach (InstalledPlugin plugin in InstalledPlugins)
            {
                if (!plugin.IsDownloaded)
                {
                    Task.Run(() => PluginManager.Instance.DownloadPluginAsync(plugin, entityViewModel));
                }
            }

            foreach (InstalledPlugin plugin in PluginManager.Instance.LoadInstalledPlugins(InstalledPlugins, entityViewModel))
            {
                InstalledPlugins.Add(plugin);
            }
            
            InstalledPlugins.CollectionChanged += InstalledPluginsChanged;
            
            Categories = new List<PluginCategory>{new PluginCategory{id=0,name="All Categories"}};
            SelectedCategory = Categories[0];
            EntityViewModel = entityViewModel;
            Sorting = PluginEnums.Sorting.RATING;
            pluginWebRequester = new PluginWebRequester();
            new Thread(() =>
            {
                var plugins = RequestPlugins();
                Application.Current.Dispatcher?.Invoke(()=> Plugins = new ObservableCollection<Plugin>(plugins));
                RemoveInstalledPluginsFromList();
                var categories = pluginWebRequester.RequestCategories();
                Application.Current.Dispatcher?.Invoke(()=> Categories.AddRange(categories));
            }){IsBackground = true}.Start();
            
        }

        public void DisablePlugin(InstalledPlugin installedPlugin)
        {
            installedPlugin.IsEnabled = false;
            RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(installedPlugin.IsEnabled)));
            InstalledPluginSerializer.Instance.StoreInstalledPlugins(InstalledPlugins.ToList(), EntityViewModel);
        }
        
        public void EnablePlugin(InstalledPlugin installedPlugin)
        {
            installedPlugin.IsEnabled = true;
            RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(installedPlugin.IsEnabled)));
            InstalledPluginSerializer.Instance.StoreInstalledPlugins(InstalledPlugins.ToList(), EntityViewModel);
        }

        public async Task<bool> Reload()
        {
            fullyLoaded = false;
            Task<bool> t = new Task<bool>(() =>
            {
                lastPage = 1;
                try
                {
                    List<Plugin> newPlugins = RequestPlugins();

                    Plugins = new ObservableCollection<Plugin>(newPlugins);
                    
                    RemoveInstalledPluginsFromList();

                    return true;
                }
                catch (Exception e)
                {
                    ErrorLogger.Append(e);
                    return false;
                }
            });
            t.Start();
            return await t;
        }

        public async Task<bool> LoadMore()
        {
            if (Plugins == null || fullyLoaded)
            {
                return false;
            }
            Task<bool> t = new Task<bool>(() =>
            {
                lastPage++;
                try
                {
                    List<Plugin> newPlugins = RequestPlugins();

                    //TODO in .NET 6.0 this can be changed to AddRange() (watch: https://github.com/dotnet/runtime/issues/18087)
                    foreach (Plugin newPlugin in newPlugins)
                    {
                        Application.Current.Dispatcher?.Invoke(() => Plugins.Add(newPlugin));
                    }
                    
                    RemoveInstalledPluginsFromList();

                    return true;
                }
                catch (Exception e)
                {
                    ErrorLogger.Append(e);
                    return false;
                }
            });
            t.Start();
            return await t;
        }

        public void CheckForDeletedPlugin(Plugin plugin)
        {
            if (plugin == null) return;
            foreach (Plugin plugin1 in Plugins)
            {
                if (plugin1.id == plugin.id)
                {
                    plugin1.installed = false;
                }
            }
        }

        private List<Plugin> RequestPlugins()
        {
            //SearchQuery and Category
            if (SelectedCategory.id == 0 && SearchQuery.Equals(""))
            {
                var result = pluginWebRequester.RequestResourceList(out fullyLoaded, lastPage, Sorting);
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(FullyLoaded)));
                return result;
            }

            //Category only
            if (SelectedCategory.id == 0)
            {
                var result1 = pluginWebRequester.RequestResourceList(out fullyLoaded, SearchQuery, null, lastPage, Sorting);
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(FullyLoaded)));
                return result1;
            }

            //SearchQuery only
            if (!SearchQuery.Equals(""))
            {
                var result2 = pluginWebRequester.RequestResourceList(out fullyLoaded, SearchQuery, SelectedCategory, lastPage, Sorting);
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(FullyLoaded)));
                return result2;
            }

            //No SearchQuery and no Category
            var result3 = pluginWebRequester.RequestResourceList(out fullyLoaded, SelectedCategory, lastPage, Sorting);
            RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(FullyLoaded)));
            return result3;
        }

        private void InstalledPluginsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //TODO re-sort
            
            RemoveInstalledPluginsFromList();
            
            InstalledPluginSerializer.Instance.StoreInstalledPlugins(InstalledPlugins.ToList(), EntityViewModel);
        }

        private void RemoveInstalledPluginsFromList()
        {
            if (Plugins == null) return;
            foreach (Plugin plugin in Plugins)
            {
                foreach (InstalledPlugin installedPlugin in InstalledPlugins)
                {
                    if (plugin.id == installedPlugin.SpigetId)
                    {
                        plugin.installed = true;
                    }
                }
            }
        }
    }
}