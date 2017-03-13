using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 第三方登录相关契约 </summary>
    public partial interface IUserContract
    {
        /// <summary> 第三方登录 </summary>
        /// <param name="platformDto"></param>
        /// <param name="comefrom"></param>
        /// <param name="partner"></param>
        /// <returns>token</returns>
        DResult<PlatformLoginResultDto> Login(PlatformDto platformDto, Comefrom comefrom, string partner = "");

        /// <summary> 创建第三方登录帐号 </summary>
        /// <param name="platId"></param>
        /// <returns></returns>
        DResult<string> CreatePlatAccount(string platId);

        /// <summary> 第三方登录帐号绑定 </summary>
        /// <param name="platId"></param>
        /// <param name="account"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        DResult<string> AccountBind(string platId, string account, string pwd);

        /// <summary> 第三方登录帐号绑定 </summary>
        /// <param name="platId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult AccountBind(string platId, long userId);

        /// <summary> 取消第三方绑定 </summary>
        /// <param name="userId"></param>
        /// <param name="platType"></param>
        /// <returns></returns>
        DResult CancelAccountBind(long userId, byte platType);

        /// <summary> 获取已绑定的第三方登录类型 </summary>
        /// <param name="userId"></param>
        /// <param name="platformType"></param>
        /// <returns></returns>
        IEnumerable<PlatformDto> Platforms(long userId, int platformType = -1);
    }
}
