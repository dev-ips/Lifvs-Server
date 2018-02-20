using System;

namespace Lifvs.Common.Utility.Interfaces
{
    public interface ICacheHelper
    {
        void AddToCache(string cacheKeyName, object cacheItem, Int32 cacheExpiration = 100000);
        object GetCachedItem(string cacheKeyName);
        void RemoveCachedItem(string cacheKeyName);
        T GetCachedItem<T>(string cacheKeyName);
    }
}
