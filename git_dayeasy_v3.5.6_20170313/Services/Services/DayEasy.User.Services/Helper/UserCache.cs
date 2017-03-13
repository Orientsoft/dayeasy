using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Cache;
using DayEasy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.User.Services.Helper
{
    /// <summary> 用户缓存 </summary>
    public class UserCache
    {
        private static readonly TimeSpan Expire = TimeSpan.FromHours(2);
        private const string UserRegion = "user";
        private const string UserTokenRegion = UserRegion + "_token";
        private const string UserAppsRegion = UserRegion + "_apps";
        private const string UserAgencyRegion = UserRegion + "_agency";

        private UserCache() { }
        /// <summary> 唯一实例 </summary>
        public static UserCache Instance
        {
            get { return Singleton<UserCache>.Instance ?? (Singleton<UserCache>.Instance = new UserCache()); }
        }

        private static string TokenKey(string token, byte comefrom)
        {
            return string.Concat(token, "_", ((Comefrom)comefrom).ToString());
        }

        public void Set(UserDto user)
        {
            if (user == null || user.Id <= 0)
                return;
            var key = user.Id.ToString();
            CacheManager.GetCacher(UserRegion).Set(key, user, Expire);
        }

        public UserDto Get(long id)
        {
            if (id <= 0)
                return null;
            var key = id.ToString();
            return CacheManager.GetCacher(UserRegion).Get<UserDto>(key);
        }

        public void Remove(long id)
        {
            if (id <= 0)
                return;
            var key = id.ToString();
            CacheManager.GetCacher(UserRegion).Remove(key);
        }

        /// <summary> 获取缓存用户信息 </summary>
        /// <param name="token"></param>
        /// <param name="comefrom"></param>
        /// <returns></returns>
        public UserDto Get(string token, byte comefrom = 0)
        {
            var key = TokenKey(token, comefrom);
            var id = CacheManager.GetCacher(UserTokenRegion).Get<long>(key);
            return Get(id);
        }

        /// <summary> 设置用户缓存 </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <param name="comefrom"></param>
        public void Set(UserDto user, string token, byte comefrom = 0)
        {
            if (user == null)
                return;
            var key = TokenKey(token, comefrom);
            var cache = CacheManager.GetCacher(UserTokenRegion);
            if (user.ExpireTime.HasValue)
                cache.Set(key, user.Id, user.ExpireTime.Value);
            else
                cache.Set(key, user.Id, Expire);
            Set(user);
        }


        /// <summary> 删除单个用户缓存信息 </summary>
        /// <param name="token"></param>
        /// <param name="comefrom"></param>
        public void Remove(string token, Comefrom comefrom = Comefrom.Web)
        {
            var key = TokenKey(token, (byte)comefrom);
            CacheManager.GetCacher(UserTokenRegion).Remove(key);
        }

        public void Remove(IDictionary<string, byte> tokens)
        {
            if (tokens == null || !tokens.Any())
                return;
            var keys = tokens.Select(t => TokenKey(t.Key, t.Value));
            CacheManager.GetCacher(UserTokenRegion).Remove(keys);
        }

        /// <summary> 获取缓存的用户应用信息 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ApplicationDto> GetApps(long userId)
        {
            var apps = new List<ApplicationDto>();
            if (userId <= 0)
                return apps;
            var key = userId.ToString();
            return CacheManager.GetCacher(UserAppsRegion).Get<List<ApplicationDto>>(key);
        }

        public void SetApps(List<ApplicationDto> apps, long userId)
        {
            var key = userId.ToString();
            apps = apps ?? new List<ApplicationDto>();
            CacheManager.GetCacher(UserAppsRegion).Set(key, apps, TimeSpan.FromDays(2));
        }

        /// <summary> 删除单个用户应用缓存 </summary>
        /// <param name="userId"></param>
        public void RemoveApps(long userId)
        {
            var key = userId.ToString();
            CacheManager.GetCacher(UserAppsRegion).Remove(key);
        }

        /// <summary> 删除所有用户引用缓存 </summary>
        public void RemoveApps()
        {
            CacheManager.GetCacher(UserAppsRegion).Clear();
        }

        /// <summary> 设置机构缓存 </summary>
        /// <param name="userId"></param>
        /// <param name="dto"></param>
        public void SetAgency(long userId, UserAgencyDto dto)
        {
            if (userId <= 0 || dto == null)
                return;
            CacheManager.GetCacher(UserAgencyRegion).Set(userId.ToString(), dto, TimeSpan.FromDays(10));
        }

        /// <summary> 获取用户机构缓存 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserAgencyDto CurrentAgency(long userId)
        {
            return CacheManager.GetCacher(UserAgencyRegion).Get<UserAgencyDto>(userId.ToString());
        }

        /// <summary> 清空用户机构缓存 </summary>
        /// <param name="userId"></param>
        public void RemoveAgency(long userId)
        {
            if (userId <= 0)
                return;
            CacheManager.GetCacher(UserAgencyRegion).Remove(userId.ToString());
        }
    }
}
