using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts.Management
{
    public partial interface IManagementContract
    {
        DResults<TG_Group> GroupSearch(GroupSearchDto searchDto);
        DResults<TG_Group> GroupSearch(List<string> groupIds);
        DResult GroupDelete(string groupId);
        DResult GroupCertificate(string groupId);

        DResult GroupStudentExcel(string groupId);

        List<TG_Share> ShareGroups(List<string> shareGroupIds);

        Dictionary<string, string> GroupTags(ICollection<string> groupIds);

        /// <summary> 搜索用户 </summary>
        /// <param name="groupId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        DResults<UserDto> SearchUsers(string groupId, string keyword);

        /// <summary> 发起协同 </summary>
        /// <param name="paperCode"></param>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="operatorId"></param>
        /// <returns></returns>
        DResult PublishJoint(string paperCode, string groupId, long userId, long operatorId);

        /// <summary> 编辑圈子信息 </summary>
        /// <param name="inputDto"></param>
        /// <returns></returns>
        DResult UpdateGroup(UpdateGroupInputDto inputDto);
    }
}
