using System;
using System.Collections.Generic;

namespace Fork.Logic.WebRequesters
{
    public class ResponseCache
    {
        /// <summary>
        /// Singleton Header
        /// </summary>
        private static ResponseCache instance;
        public static ResponseCache Instance
        {
            get
            {
                if (instance==null)
                {
                    instance = new ResponseCache();
                }
                return instance;
            }
        }
        private ResponseCache(){}

        /// <summary>
        /// Attributes
        /// </summary>
        private int maxCacheAgeHours = 2;
        private Dictionary<string, Tuple<string, DateTime>> cachedResponses = new Dictionary<string, Tuple<string, DateTime>>();

        /// <summary>
        /// Methods
        /// </summary>
        public void CacheResponse(string URL, string Response)
        {
            cachedResponses[URL] = new Tuple<string, DateTime>(Response,DateTime.Now);
        }
        
        public string UncacheResponse(string URL)
        {
            if (!cachedResponses.ContainsKey(URL))
            {
                return null;
            }
            var responseTuple = cachedResponses[URL];
            if (!VerifyCacheAge(responseTuple.Item2))
            {
                return null;
            }
            return responseTuple.Item1;
        }

        private bool VerifyCacheAge(DateTime cacheAge)
        {
            TimeSpan difference = DateTime.Now - cacheAge;
            if (difference.TotalHours < maxCacheAgeHours)
            {
                return true;
            }
            return false;
        }
    }
}