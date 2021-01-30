using System;
using System.Net;
using System.Timers;
using Fork.Logic.Controller;

namespace Fork.Logic.Model.APIModels
{
    public class CachedAPIResponse
    {
        private Timer timer;
            
        public HttpWebResponse WebResponse { get; }
        public TimeSpan MaxCacheAge { get; }

        public CachedAPIResponse(string requestHash, HttpWebResponse webResponse, TimeSpan maxCacheAge)
        {
            WebResponse = webResponse;
            MaxCacheAge = maxCacheAge;

            timer = new Timer
            {
                AutoReset = false,
                Interval = maxCacheAge.TotalMilliseconds,
                Enabled = true
            };
            timer.Elapsed += (_, _) =>
            {
                if (APIController.ResponseCache.ContainsKey(requestHash))
                {
                    APIController.ResponseCache.Remove(requestHash);
                }
                timer.Dispose();
            };
        }
    }
}