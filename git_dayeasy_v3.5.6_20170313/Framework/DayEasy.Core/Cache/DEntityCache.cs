using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Core.Cache
{
    /// <summary> Redis缓存基类 </summary>
    public abstract class DEntityCache<T>
        where T : DDto
    {
        /// <summary> 有效时间 </summary>
        private readonly TimeSpan _expire;

        /// <summary> 构造函数 </summary>
        /// <param name="region"></param>
        /// <param name="expire"></param>
        protected DEntityCache(string region = null, TimeSpan? expire = null)
        {
            region = region ?? typeof(T).Name;
            _expire = expire ?? TimeSpan.FromDays(30);
            Cache = CacheManager.GetCacher(region);
        }

        /// <summary> 获取缓存 </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T Get(string key)
        {
            return Cache.Get<T>(key);
        }

        /// <summary> 设置缓存 </summary>
        /// <param name="dto"></param>
        /// <param name="key"></param>
        public virtual void Set(T dto, string key)
        {
            if (dto == null || string.IsNullOrWhiteSpace(key))
                return;
            Cache.Set(key, dto, _expire);
        }

        /// <summary> 删除缓存 </summary>
        /// <param name="key"></param>
        public virtual void Remove(string key)
        {
            Cache.Remove(key);
        }
        /// <summary> 缓存实例 </summary>
        protected ICache Cache { get; }
    }
}
