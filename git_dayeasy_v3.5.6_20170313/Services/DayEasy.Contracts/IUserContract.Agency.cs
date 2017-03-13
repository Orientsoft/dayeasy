
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IUserContract
    {
        /// <summary> 添加机构履历 </summary>
        DResult AddAgency(UserAgencyInputDto dto);

        /// <summary> 删除履历 </summary>
        DResult RemoveAgency(string id, long userId);

        /// <summary> 更新关系 </summary>
        /// <returns></returns>
        DResult UpdateRelation(UpdateRelationInputDto dto);

        /// <summary> 个人履历 </summary>
        DResults<UserAgencyDto> AgencyList(long userId, DPage page);
        /// <summary> 用户当前机构 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<UserAgencyDto> CurrentAgency(long userId);
        /// <summary>
        /// 根据机构ID获取用户信息
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="userRole"></param>
        /// <param name="subjectId">科目</param>
        /// <returns></returns>
        DResults<UserDto> LoadUsersByAgencyId(string agencyId, int userRole = -1, int subjectId = -1);
    }
}
