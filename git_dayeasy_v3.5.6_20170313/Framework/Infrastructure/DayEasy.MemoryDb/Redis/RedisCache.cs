using DayEasy.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.MemoryDb.Redis
{
    /// <summary> Redis分布式缓存 </summary>
    public class RedisCache : DCache
    {
        private readonly string _region;
        private readonly RedisManager _manager;
        public RedisCache(string region)
        {
            _region = region;
            _manager = RedisManager.Instance;
        }

        public override string Region
        {
            get { return _region; }
        }

        public override void Set(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return;
            var cacheKey = GetKey(key);
            using (var cache = _manager.CacheClient)
            {
                cache.Set(cacheKey, value);
            }
        }

        public override void Set(string key, object value, TimeSpan expire)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return;
            var cacheKey = GetKey(key);
            using (var cache = _manager.CacheClient)
            {
                cache.Set(cacheKey, value, expire);
            }
        }

        public override void Set(string key, object value, DateTime expire)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return;
            var cacheKey = GetKey(key);
            using (var cache = _manager.CacheClient)
            {
                cache.Set(cacheKey, value, expire);
            }
        }

        public override object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            var cacheKey = GetKey(key);
            using (var cache = _manager.CacheClient)
            {
                return cache.Get<object>(cacheKey);
            }
        }

        public override IEnumerable<object> GetAll()
        {
            var token = string.Concat(_region, ":");
            using (var cache = _manager.ReadOnlyClient())
            {
                var keys = cache.SearchKeys(token + "*");
                return cache.GetValues<object>(keys);
            }
        }

        public override T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return default(T);
            var cacheKey = GetKey(key);
            using (var cache = _manager.CacheClient)
            {
                return cache.Get<T>(cacheKey);
            }
        }

        public override void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;
            var cacheKey = GetKey(key);
            using (var cache = _manager.CacheClient)
            {
                cache.Remove(cacheKey);
            }
        }

        public override void Remove(IEnumerable<string> keys)
        {
            var cacheKeys = keys.Select(GetKey);
            using (var cache = _manager.CacheClient)
            {
                cache.RemoveAll(cacheKeys);
            }
        }

        public override void Clear()
        {
            var token = _region + ":";
            using (var cache = _manager.GetClient())
            {
                var cacheKeys = cache.SearchKeys(token + "*");
                if (cacheKeys != null && cacheKeys.Any())
                {
                    cache.RemoveAll(cacheKeys);
                }
            }
        }
    }
}
