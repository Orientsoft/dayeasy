using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contracts
{
    /// <summary> 圈子基础契约接口 </summary>
    public partial interface IGroupContract : IDependency
    {
        /// <summary> 创建圈子 </summary>
        /// <param name="groupDto"></param>
        /// <param name="creatorRole"></param>
        /// <param name="trueName"></param>
        /// <returns></returns>
        DResult<GroupDto> CreateGroup(GroupDto groupDto, byte creatorRole = (byte)UserRole.Teacher,
            string trueName = null, bool isManage = false);
        /// <summary>
        /// 指定权限创建圈子
        /// </summary>
        /// <param name="groupDto">圈子信息</param>
        /// <param name="status">权限状态码</param>
        /// <param name="appId">应用编号</param>
        /// <returns></returns>
        DResult<GroupDto> CreateGroupForManage(GroupDto groupDto, int status, int appId);
        /// <summary>
        /// 批量创建圈子
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        DResult BatchCreateGroups(BatchCreateGroupsDto dto, string agencyId);

        /// <summary> 获取用户所有圈子 </summary>
        /// <param name="userId"></param>
        /// <param name="groupType"></param>
        /// <param name="loadMessageCount">加载消息数量</param>
        /// <param name="containsAll"></param>
        /// <returns></returns>
        DResults<GroupDto> Groups(long userId, int groupType = -1, bool loadMessageCount = false, bool containsAll = false);

        /// <summary> 获取班级圈子的毕业年级 </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<Dictionary<int, List<int>>> GroupGradeYears(long userId);

        /// <summary> 根据圈号查询圈子 </summary>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        DResult<GroupDto> LoadByCode(string groupCode);

        /// <summary> 根据圈号查询圈子 </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        DResult<GroupDto> LoadById(string groupId);

        /// <summary> 查询圈子 </summary>
        /// <param name="searchGroupDto"></param>
        /// <returns></returns>
        DResults<GroupDto> SearchGroups(SearchGroupDto searchGroupDto);

        /// <summary> 查询圈子 </summary>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        DResults<GroupDto> SearchGroups(List<string> groupIds);

        /// <summary> 查询圈子基础信息 </summary>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        Dictionary<string, DGroupDto> GroupDtoDict(ICollection<string> groupIds);

        /// <summary> 查询圈子 </summary>
        DResults<GroupDto> SearchGroupsByCode(List<string> codes);

        /// <summary> 查询分享圈子 </summary>
        /// <param name="channelId"></param>
        /// <param name="order"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        DResults<GroupDto> SearchShareGroups(int channelId, ShareGroupOrder order, DPage page);

        /// <summary>
        /// 查询圈子类型
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        DResult<byte> GetGroupType(string groupId);

        /// <summary> 查询所有圈号 </summary>
        IEnumerable<string> GroupCodes();

        /// <summary> 更新圈子设置 </summary>
        /// <param name="updateDto"></param>
        /// <returns></returns>
        DResult Update(UpdateGroupDto updateDto);

        /// <summary> 转让圈主 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="operatorId"></param>
        /// <returns></returns>
        DResult TransferOwner(string groupId, long userId, long operatorId);

        /// <summary> 解散圈子 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult DissolutionGroup(string groupId, long userId);

        /// <summary> 认证的机构同事圈 </summary>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        List<string> AgencyColleagueGroups(string agencyId);

        /// <summary> 获取圈子列表 </summary>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        Dictionary<string, string> GroupDict(List<string> groupIds);

        /// <summary> 是否是管理员 </summary>
        /// <param name="group"></param>
        /// <param name="userId"></param>
        bool IsManager(GroupDto group, long userId);
        /// <summary>
        /// 圈子认证
        /// </summary>
        /// <param name="groupId"></param>
        ///  <param name="isAuth"></param>
        /// <returns></returns>
        DResult GroupCertificate(string groupId, bool isAuth);

        ///  <summary>
        /// 圈子批量导入学生
        ///  </summary>
        ///  <param name="students">用户列表</param>
        ///  <param name="groupId">圈子ID</param>
        ///  <param name="agencyId">机构ID</param>
        /// <param name="stage"></param>
        /// <returns></returns>
        DResult BatchImportStudent(string[] students, string groupId, string agencyId, byte stage);
        /// <summary>
        /// 班级圈批量导入教师
        /// </summary>
        /// <param name="tIds">教师ID</param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        DResult BatchImportTeacher(long[] tIds, string groupId);
        /// <summary>
        /// 同事圈批量导入教师
        /// </summary>
        /// <param name="tIds"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        DResult ColleagueBatchTeacher(long[] tIds, string groupId);
        /// <summary>
        /// 获取重复的圈子信息
        /// </summary>
        /// <returns></returns>
        DResult<OutGroupMessage> GetGroupRepeatMsg(BatchCreateGroupsDto dto, string agencyId);
        /// <summary>
        ///  Execel导入学生
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="groupId">圈子ID</param>
        /// <param name="agencyId">机构ID</param>
        /// <param name="stage">学段号</param>
        /// <returns></returns>
        DResult<OutGroupMessage> ExecelImportSutdents(string path, string groupId, string agencyId, int stage);

    }
}
