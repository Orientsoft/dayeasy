using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Core.Cache;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using DayEasy.Utility.Timing;
using System;

namespace DayEasy.Statistic.Services.Helper
{
    /// <summary> 统计缓存辅助 </summary>
    public class StatisticCacheHelper
    {
        private const string Region = "statistic";
        private readonly ICache _cache;

        private StatisticCacheHelper()
        {
            _cache = CacheManager.GetCacher(Region);
        }

        /// <summary> 单一实例 </summary>
        public static StatisticCacheHelper Instance
        {
            get
            {
                return (Singleton<StatisticCacheHelper>.Instance ??
                        (Singleton<StatisticCacheHelper>.Instance = new StatisticCacheHelper()));
            }
        }

        /// <summary> 获取缓存值，以及缓存时间 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        public void Set<T>(string key, T value, TimeSpan timespan)
        {
            if (timespan == TimeSpan.Zero)
                _cache.Set(key, new DKeyValue<T, DateTime>(value, Clock.Now));
            else
                _cache.Set(key, new DKeyValue<T, DateTime>(value, Clock.Now), timespan);
        }

        public void SetByRefreshTime<T>(string key, T value, TimeSpan timespan)
            where T : RefreshDto
        {
            value.LastRefresh = Clock.Now;
            if (timespan == TimeSpan.Zero)
                _cache.Set(key, value);
            else
                _cache.Set(key, value, timespan);
        }

        /// <summary> 清空缓存 </summary>
        /// <param name="key"></param>
        public void Clear(string key)
        {
            _cache.Remove(key);
        }
    }
}
