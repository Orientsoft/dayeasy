using DayEasy.Contracts.Dtos.User;
using DayEasy.User.Services.Helper;

namespace DayEasy.User.Services
{
    public partial class UserService
    {
        public void ResetCache(long userId)
        {
            UserCache.Instance.Remove(userId);
        }

        public void ResetAppCache(long userId = -1)
        {
            if (userId > 0)
                UserCache.Instance.RemoveApps(userId);
            else
                UserCache.Instance.RemoveApps();
        }

        public UserDto LoadFromCache(long userId)
        {
            var cache = UserCache.Instance;
            var user = cache.Get(userId);
            if (user == null)
            {
                user = UserRepository.Load(userId).ToDto();
                cache.Set(user);
            }
            return user;
        }
    }
}
