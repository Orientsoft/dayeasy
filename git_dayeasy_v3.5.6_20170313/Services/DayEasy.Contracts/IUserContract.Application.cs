using System.Collections.Generic;
using DayEasy.Contracts.Dtos;
using DayEasy.Utility;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts
{
    /// <summary> 用户业务模块 - 应用相关业务 </summary>
    public partial interface IUserContract
    {
        /// <summary> 加载用户应用 </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="fromCache">从缓存加载</param>
        /// <returns></returns>
        List<ApplicationDto> UserApplications(long userId, bool fromCache = true);

        /// <summary> 添加用户App </summary>
        /// <param name="userId"></param>
        /// <param name="applicationId"></param>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        DResult AddApplication(long userId, int applicationId, string agencyId = null);
        /// <summary> 应用的机构权限 </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        DResult<AgencyDto> ApplicationAgency(long userId, int appId);
    }
}
