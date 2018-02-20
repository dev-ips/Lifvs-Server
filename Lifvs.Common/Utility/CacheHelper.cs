using Lifvs.Common.Utility.Interfaces;
using System;
using System.Runtime.Caching;

namespace Lifvs.Common.Utility
{
    public class CacheHelper : ICacheHelper
    {
        private static ObjectCache cache = MemoryCache.Default;
        private CacheItemPolicy policy = null;
        private CacheEntryRemovedCallback callback = null;

        public void AddToCache(string cacheKeyName, object cacheItem, Int32 cacheExpiration)
        {
            policy = new CacheItemPolicy
            {
                Priority = (CacheItemPriority.Default),
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(cacheExpiration),
                RemovedCallback = callback
            };

            cache.Set(cacheKeyName, cacheItem, policy);
        }

        public object GetCachedItem(string cacheKeyName)
        {
            return cache[cacheKeyName] as Object;

        }

        public void RemoveCachedItem(string cacheKeyName)
        {
            if (cache.Contains(cacheKeyName))
            {
                cache.Remove(cacheKeyName);
            }
        }

        public T GetCachedItem<T>(string cacheKeyName)
        {
            throw new NotImplementedException();
        }
    }
}
