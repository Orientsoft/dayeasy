using System;
using DayEasy.AutoMapper.Attributes;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary>
    /// 兼容老版本 - 不要再使用此用户类
    /// </summary>
    [AutoMap(typeof(UserDto))]
    public class VuUserInfo
    {
        public long UserId { get; set; }

        public string Email { get; set; }

        public string NickName { get; set; }

        public string RealName { get; set; }

        public string HeadPic { get; set; }

        public int Status { get; set; }

        public string DisplayName { get; set; }

        public DateTime AddedAt { get; set; }

        public string Mobile { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        public byte Role { get; set; }

        /// <summary>
        /// 学科ID
        /// </summary>
        public int SubjectId { get; set; }

        /// <summary>
        /// 科目名称
        /// </summary>
        public string SubjectName { get; set; }

        /// <summary>
        /// 当前机构ID
        /// </summary>
        public string AgencyId { get; set; }

        public string AgencyName { get; set; }

        /// <summary>
        /// 机构座右铭
        /// </summary>
        public string AgencyMotto { get; set; }
        /// <summary>
        /// 机构Logo
        /// </summary>
        public string AgencyLogo { get; set; }

        public byte ValidationType { get; set; }

        public string StudentNo { get; set; }

        public DateTime? ExpireTime { get; set; }

    }

    public static class UserModelClassConvert
    {
        public static UserDto ToUserDto(VuUserInfo user)
        {
            return new UserDto
            {
                Id = user.UserId,
                Name = user.RealName,
                Nick = user.NickName,
                Avatar = user.HeadPic,
                Email = user.Email,
                Mobile = user.Mobile,
                Role = user.Role,
                SubjectId = user.SubjectId,
                SubjectName = user.SubjectName,
                StudentNum = user.StudentNo,
                ExpireTime = user.ExpireTime
            };
        }

        public static VuUserInfo ToVuUserInfo(UserDto user)
        {
            return new VuUserInfo
            {
                UserId = user.Id,
                RealName = user.Name,
                NickName = user.Nick,
                HeadPic = user.Avatar,
                Email = user.Email,
                Mobile = user.Mobile,
                Role = user.Role,
                SubjectId = user.SubjectId,
                SubjectName = user.SubjectName,
                StudentNo = user.StudentNum,
                ExpireTime = user.ExpireTime
            };
        }
    }
}
