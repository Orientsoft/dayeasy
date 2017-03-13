using System.Collections.Concurrent;
using DayEasy.Core.Cache;
using DayEasy.MemoryDb.Redis;

namespace DayEasy.Framework.Cache
{
    /// <summary> Redis分布式缓存提供者 </summary>
    public class RedisCacheProvider : ICacheProvider
    {
        private static readonly ConcurrentDictionary<string, ICache> Caches;

        static RedisCacheProvider()
        {
            Caches = new ConcurrentDictionary<string, ICache>();
        }

        /// <summary>
        /// 获取 缓存是否可用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="regionName">缓存区域名称</param>
        /// <returns></returns>
        public ICache GetCache(string regionName)
        {
            ICache cache;
            if (Caches.TryGetValue(regionName, out cache))
            {
                return cache;
            }
            cache = new RedisCache(regionName);
            Caches[regionName] = cache;
            return cache;
        }
    }
}
