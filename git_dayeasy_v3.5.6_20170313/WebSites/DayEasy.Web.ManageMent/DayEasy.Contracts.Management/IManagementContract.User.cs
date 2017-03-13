
using System.Collections.Generic;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Utility;

namespace DayEasy.Contracts.Management
{
    /// <summary> 用户管理 </summary>
    public partial interface IManagementContract
    {
        long AdminRole(long userId);

        DResult SetAdminRole(long userId, long adminRole);

        DResults<TU_User> UserSearch(UserSearchDto searchDto);
        DResults<TU_User> UserSearch(List<long> userIds);

        DResult UserDelete(long userId);

        DResult ResetPassword(long userId);

        DResult UserEdit(long userId, string realName);

        DResult<UserActiveDto> UserActiveInfo(long userId);

        DResults<UserManagerDto> ManagerSearch(ManagerSearchDto searchDto);

        DResult SetManager(string keyword);

        DResult RemoveManager(long userId);

        DResult UpdateManager(long userId, long role);

        DResult ImportStudentNums(Dictionary<string, string> studentDict);

        DResult CertificateUser(long userId);
    }
}
