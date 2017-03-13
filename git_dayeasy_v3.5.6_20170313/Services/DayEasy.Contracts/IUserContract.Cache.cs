
using DayEasy.Contracts.Dtos.User;

namespace DayEasy.Contracts
{
    public partial interface IUserContract
    {
        /// <summary> 重置用户缓存 </summary>
        /// <param name="userId"></param>
        void ResetCache(long userId);

        /// <summary> 重置用户应用缓存 </summary>
        /// <param name="userId"></param>
        void ResetAppCache(long userId = -1);

        /// <summary> 从缓存加载用户信息 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UserDto LoadFromCache(long userId);
    }
}
