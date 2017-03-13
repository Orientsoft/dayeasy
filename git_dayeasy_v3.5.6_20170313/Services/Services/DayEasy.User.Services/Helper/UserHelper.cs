using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Services.Configs;
using DayEasy.Services.Helper;
using DayEasy.Utility.Config;

namespace DayEasy.User.Services.Helper
{
    internal static class UserHelper
    {
        internal const string LoginDefaultMsg = "系统维护中，暂时无法登录，详情请咨询客服人员！";


        /// <summary> 禁止登录列表 </summary>
        /// <returns></returns>
        public static LoginForbiddenConfig ForbiddenList()
        {
            return ConfigUtils<LoginForbiddenConfig>.Instance.Get();
        }

        public static bool HasRole(int role, UserRole userRole)
        {
            return role > 0 && (role & (int)userRole) > 0;
        }

        /// <summary> 用户信息与站点用户转换 </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static UserDto ToDto(this TU_User user)
        {
            if (user == null)
                return null;
            var dto = user.MapTo<UserDto>();
            if (HasRole(dto.Role, UserRole.Teacher) && dto.SubjectId > 0)
            {
                dto.SubjectName = SystemCache.Instance.SubjectName(dto.SubjectId);
            }
            if (string.IsNullOrWhiteSpace(dto.Avatar))
                dto.Avatar = Consts.DefaultAvatar();
            return dto;
        }
    }
}
