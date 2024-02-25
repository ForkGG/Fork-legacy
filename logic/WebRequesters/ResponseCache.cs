using System;
using System.Collections.Generic;

namespace Fork.Logic.WebRequesters;

public class ResponseCache
{
    /// <summary>
    ///     Singleton Header
    /// </summary>
    private static ResponseCache instance;

    private readonly Dictionary<string, Tuple<string, DateTime>> cachedResponses = new();

    /// <summary>
    ///     Attributes
    /// </summary>
    private readonly int maxCacheAgeHours = 2;

    private ResponseCache()
    {
    }

    public static ResponseCache Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ResponseCache();
            }

            return instance;
        }
    }

    /// <summary>
    ///     Methods
    /// </summary>
    public void CacheResponse(string URL, string Response)
    {
        lock (cachedResponses)
        {
            cachedResponses[URL] = new Tuple<string, DateTime>(Response, DateTime.Now);
        }
    }

    public string UncacheResponse(string URL)
    {
        lock (cachedResponses)
        {
            if (!cachedResponses.ContainsKey(URL))
            {
                return null;
            }

            Tuple<string, DateTime> responseTuple = cachedResponses[URL];
            if (!VerifyCacheAge(responseTuple.Item2))
            {
                return null;
            }

            return responseTuple.Item1;
        }
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