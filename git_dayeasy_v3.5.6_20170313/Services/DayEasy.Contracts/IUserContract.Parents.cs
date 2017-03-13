using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    public partial interface IUserContract
    {
        /// <summary> 获取学生信息 </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        DResult<DUserDto> LoadChild(string account, string password);

        /// <summary> 第三方登录获取学生信息 </summary>
        /// <param name="platformDto"></param>
        /// <returns></returns>
        DResult<DUserDto> LoadChildByPlatform(PlatformDto platformDto);

        /// <summary> 绑定孩子 </summary>
        /// <param name="parentsId"></param>
        /// <param name="studentId"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        DResult BindChild(long parentsId, long studentId, FamilyRelationType relationType);

        /// <summary> 取消关联 </summary>
        /// <param name="parentId"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult CancelBindRelation(long parentId, long studentId);

        /// <summary> 获取家长帐号 </summary>
        /// <param name="childId"></param>
        List<RelationUserDto> Parents(long childId);

        /// <summary> 查找学生家属 </summary>
        /// <param name="studentIds"></param>
        /// <returns></returns>
        Dictionary<long, List<RelationUserDto>> ParentsDict(List<long> studentIds);

        /// <summary> 获取孩子信息 </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        List<RelationUserDto> Children(long parentId);
    }
}
