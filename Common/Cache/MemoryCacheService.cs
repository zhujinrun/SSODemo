using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool StringSet<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (expiry == null)
            {
                _memoryCache.Set(key, value);
            }
            else
            {
                _memoryCache.Set(key, value, (TimeSpan)expiry);
            }
            return true;
        }

        public T StringGet<T>(string key)
        {
            T result = _memoryCache.Get<T>(key);
            return result;
        }

        public void DeleteKey(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}
