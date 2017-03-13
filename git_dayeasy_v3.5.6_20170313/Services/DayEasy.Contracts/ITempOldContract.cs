using System.Collections.Generic;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 临时契约 </summary>
    public interface ITempOldContract : IDependency
    {
        DResults<TU_Agency> AgencyList(DPage page);

        /// <summary> 用户是否在某些机构 </summary>
        /// <param name="userId"></param>
        /// <param name="agencyIds"></param>
        /// <returns></returns>
        bool InAgencies(long userId, IEnumerable<string> agencyIds);

        /// <summary> 批量导入用户 </summary>
        /// <param name="users"></param>
        /// <param name="relations"></param>
        /// <returns></returns>
        int ImportUsers(IEnumerable<TU_User> users, IEnumerable<TU_UserAgencyRelation> relations);

        DResults<TU_Class> ClassList(DPage page);

        DResults<TU_UserAgencyRelation> RelationList(string groupId);

        DResult<int> UpdateDCode();

        DResult AddApplication(long userId, int applicationId);

        List<TS_DynamicNews> LoadDynamics(int page, int size);
    }
}
