using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Fork.logic.model.PluginModels;
using Fork.Logic.Logging;
using Fork.Logic.Manager;
using Fork.Logic.Model.PluginModels;
using Newtonsoft.Json;

namespace Fork.Logic.WebRequesters
{
    public class PluginWebRequester
    {
        private int pageSize = 10;
        private string baseURL = "http://api.spiget.org/v2/";
        private string resourceFields = "id%2Cname%2Ctag%2Ccontributors%2Clikes%2Cfile%2CtestedVersions%2Crating%2Cauthor%2Ccategory%2CreleaseDate%2Cdownloads%2Cicon%2Cpremium%2CupdateDate";

        public List<Plugin> RequestResourceList(out bool fullyLoaded, int page = 1, PluginEnums.Sorting sort = PluginEnums.Sorting.RATING)
        {
            string url = BuildResourceURL(pageSize,page, sort);
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                } catch(WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Spigot Plugins. (either spiget.org is down or your Internet connection is not working)\nRequest URL: "+url);
                    fullyLoaded = true;
                    return new List<Plugin>();
                }                                
            }

            var plugins = JsonConvert.DeserializeObject<List<Plugin>>(json);
            foreach (Plugin plugin in plugins)
            {
                plugin.author = RequestAuthor(plugin.author.id);
                plugin.category = RequestCategory(plugin.category.id);
            }

            fullyLoaded = plugins.Count < pageSize;

            return plugins;
        }

        public List<Plugin> RequestResourceList(out bool fullyLoaded, PluginCategory category, int page = 1, PluginEnums.Sorting sort = PluginEnums.Sorting.RATING)
        {
            string url = BuildResourceCategoryURL(pageSize * 2, category, page, sort);
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                } catch(WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Spigot Plugins. (either spiget.org is down or your Internet connection is not working)\nRequest URL: "+url);
                    fullyLoaded = true;
                    return new List<Plugin>();
                }                                
            }

            var plugins = JsonConvert.DeserializeObject<List<Plugin>>(json);
            List<Plugin> toRemove = new List<Plugin>();
            foreach (Plugin plugin in plugins)
            {
                if (plugin.premium)
                {
                    toRemove.Add(plugin);
                    continue;
                }
                plugin.author = RequestAuthor(plugin.author.id);
                plugin.category = RequestCategory(plugin.category.id);
            }

            foreach (Plugin plugin in toRemove)
            {
                plugins.Remove(plugin);
            }
            
            fullyLoaded = plugins.Count < pageSize*2;

            return plugins;
        }
        
        public List<Plugin> RequestResourceList(out bool fullyLoaded, string searchQuery, PluginCategory category = null, int page = 1, PluginEnums.Sorting sort = PluginEnums.Sorting.RATING)
        {
            string url = BuildResourceSearchURL(pageSize*5, searchQuery, page, sort);
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                } catch(WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Spigot Plugins. (either spiget.org is down or your Internet connection is not working)\nRequest URL: "+url);
                    fullyLoaded = true;
                    return new List<Plugin>();
                }                                
            }

            var plugins = JsonConvert.DeserializeObject<List<Plugin>>(json);
            List<Plugin> toRemove = new List<Plugin>();
            foreach (Plugin plugin in plugins)
            {
                if (plugin.premium)
                {
                    toRemove.Add(plugin);
                    continue;
                }

                if (category != null && plugin.category.id != category.id)
                {
                    toRemove.Add(plugin);
                    continue;
                }
                plugin.author = RequestAuthor(plugin.author.id);
                plugin.category = RequestCategory(plugin.category.id);
            }

            foreach (Plugin plugin in toRemove)
            {
                plugins.Remove(plugin);
            }

            fullyLoaded = plugins.Count < pageSize*5;
            
            return plugins;
        }

        private Author RequestAuthor(int authorId)
        {
            string url = baseURL + "authors/"+authorId;
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                } catch(WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Spigot Plugin Author. (either spiget.org is down or your Internet connection is not working)\nRequest URL: "+url);
                    return new Author{id = authorId, name = ""};
                } 
            }

            return JsonConvert.DeserializeObject<Author>(json);
        }

        public List<PluginCategory> RequestCategories()
        {
            string url = baseURL + "categories?size=100";
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                }
                catch (WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Spigot Plugin Author. (either spiget.org is down or your Internet connection is not working)\nRequest URL: " + url);
                    return new List<PluginCategory>();
                }
            }

            return JsonConvert.DeserializeObject<List<PluginCategory>>(json);
        }

        public Plugin RequestPlugin(int pluginId)
        {
            string url = baseURL + "resources/" + pluginId+"?fields="+resourceFields;
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                }
                catch (WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Spigot Plugin Details. (either spiget.org is down or your Internet connection is not working)\nRequest URL: " + url);
                    return null;
                }
            }
            return JsonConvert.DeserializeObject<Plugin>(json);
        }
        
        public long RequestLatestVersion(int pluginId)
        {
            string url = baseURL + "resources/" + pluginId+"/versions/latest";
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                }
                catch (WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive latest Spigot Plugin Version. (either spiget.org is down or your Internet connection is not working)\nRequest URL: " + url);
                    return 0;
                }
            }
            return JsonConvert.DeserializeObject<PluginVersion>(json).date;
        }

        private Category RequestCategory(int categoryId)
        {
            string url = baseURL + "categories/"+categoryId;
            string json = ResponseCache.Instance.UncacheResponse(url);
            if (json == null)
            {
                try
                {
                    Uri uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.UserAgent = ApplicationManager.UserAgent;
                    using (var response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }

                    ResponseCache.Instance.CacheResponse(url, json);
                } catch(WebException e)
                {
                    ErrorLogger.Append(e);
                    Console.WriteLine("Could not receive Spigot Plugin Author. (either spiget.org is down or your Internet connection is not working)\nRequest URL: "+url);
                    return new Category{id = categoryId, name = ""};
                } 
            }

            return JsonConvert.DeserializeObject<Category>(json);
        }

        public string BuildDownloadURL(InstalledPlugin plugin)
        {
            if (plugin.Plugin.file.type.Equals("external"))
            {
                throw new ArgumentException("Download links for external plugins can not be built");
            }

            string url = baseURL + "resources/" + plugin.SpigetId + "/download";

            return url;
        }

        private string BuildResourceURL(int pageSize, int page = 1, PluginEnums.Sorting sort = PluginEnums.Sorting.RATING)
        {
            string URL = baseURL + "resources/free";

            URL += "?size=" + pageSize;
            if (page != 1)
            {
                URL += "&page="+page;
            }
            
            URL += "&sort=" + WebUtility.HtmlEncode(sort.APIName());
            URL += "&fields=" + resourceFields;

            return URL;
        }
        
        private string BuildResourceCategoryURL(int pageSize, PluginCategory category, int page = 1, PluginEnums.Sorting sort = PluginEnums.Sorting.RATING)
        {
            string URL = baseURL + "categories/"+category.id+"/resources";

            URL += "?size=" + pageSize;
            if (page != 1)
            {
                URL += "&page="+page;
            }
            
            URL += "&sort=" + WebUtility.HtmlEncode(sort.APIName());
            URL += "&fields=" + resourceFields;
            

            return URL;
        }
        
        private string BuildResourceSearchURL(int pageSize, string searchQuery, int page = 1, PluginEnums.Sorting sort = PluginEnums.Sorting.RATING)
        {
            string URL = baseURL + "search/resources/"+searchQuery;

            URL += "?size=" + pageSize;
            if (page != 1)
            {
                URL += "&page="+page;
            }
            
            URL += "&sort=" + WebUtility.HtmlEncode(sort.APIName());
            URL += "&fields=" + resourceFields;
            

            return URL;
        }
    }
}