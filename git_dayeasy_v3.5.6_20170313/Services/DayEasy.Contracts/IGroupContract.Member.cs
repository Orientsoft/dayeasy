
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts
{
    /// <summary> 圈子成员相关接口 </summary>
    public partial interface IGroupContract
    {
        /// <summary> 申请加入圈子 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        DResult ApplyGroup(string groupId, long userId, string message, string name = null);

        /// <summary> 审核圈子申请 </summary>
        /// <param name="recordId"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        DResult Verify(string recordId, CheckStatus status, string message = null);

        /// <summary> 移除圈成员 </summary>
        /// <param name="groupId"></param>
        /// <param name="memberId"></param>
        /// <param name="operateId"></param>
        /// <returns></returns>
        DResult DeleteMember(string groupId, long memberId, long operateId);

        /// <summary> 圈成员 </summary>
        /// <param name="groupId"></param> 
        /// <param name="role">游民 = 不限</param>
        /// <param name="includeParents">是否包含父母</param>
        /// <param name="page">分页</param>
        /// <returns></returns>
        DResults<MemberDto> GroupMembers(string groupId, UserRole role = UserRole.Caird, bool includeParents = false,
            DPage page = null);

        /// <summary>
        /// 圈子成员数量
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="role">游民 = 不限</param>
        /// <returns></returns>
        int GroupMemberCount(string groupId, UserRole role = UserRole.Caird);

        /// <summary> 获取圈子成员数 </summary>
        /// <param name="groupIds"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Dictionary<string, int> GroupMemberCounts(IEnumerable<string> groupIds, UserRole role = UserRole.Caird);

        /// <summary> 是否是圈子成员 </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        CheckStatus IsGroupMember(long userId, string groupId);

        /// <summary> 退出圈子 </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        DResult QuitGroup(long userId, string groupId);

        /// <summary> 待审核列表 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResults<PendingUserDto> PendingList(string groupId, long userId);

        /// <summary> 更新成员最后读取动态时间 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        void UpdateLastTime(string groupId, long userId);

        /// <summary> 加入圈子 (公开圈子直接加入)</summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        DResult JoinGroup(string groupId, long userId);

        /// <summary> 批量添加圈子成员 </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <param name="operatorId"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        DResults<long> AddMembers(string groupId, long[] userIds, long operatorId, string remark = null);
    }
}
