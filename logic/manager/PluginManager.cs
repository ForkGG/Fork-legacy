using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fork.logic.model.PluginModels;
using Fork.Logic.Persistence;
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

        public delegate void HandlePluginAddedEvent(InstalledPlugin addedPlugin);
        public delegate void HandlePluginRemoveEvent(InstalledPlugin removedPlugin);
        public delegate void HandlePluginChangedEvent(InstalledPlugin changedPlugin);
        public event HandlePluginAddedEvent PluginAddedEvent;
        public event HandlePluginRemoveEvent PluginRemovedEvent;
        public event HandlePluginChangedEvent PluginChangedEvent;

        public async Task<bool> DownloadPluginAsync(InstalledPlugin plugin, EntityViewModel viewModel)
        {
            Task<bool> t = new Task<bool>(() => DownloadPlugin(plugin, viewModel));
            t.Start();
            bool r = await t;
            return r;
        }



        private bool DownloadPlugin(InstalledPlugin plugin, EntityViewModel viewModel)
        {
            if (plugin.IsDownloaded)
            {
                return true;
            }

            if (!plugin.IsSpigetPlugin || plugin.Plugin.file.type.Equals("external"))
            {
                return false;
            }
            
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(new PluginWebRequester().BuildDownloadURL(plugin)),
                Path.Combine(App.ServerPath, viewModel.Entity.Name, "plugins", plugin.Name, ".jar"));
            plugin.IsDownloaded = true;
            return true;
        }
    }
}