using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using Newtonsoft.Json;
using System;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 用户传输对象 </summary>
    [AutoMapFrom(typeof(TU_User))]
    public class UserDto : DUserDto
    {
        public string Email { get; set; }

        public string Mobile { get; set; }

        public byte Role { get; set; }

        [MapFrom("UserCode")]
        public string Code { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; }
        public string StudentNum { get; set; }

        public DateTime AddedAt { get; set; }

        public byte ValidationType { get; set; }
        /// <summary> 认证等级 </summary>
        [MapFrom("CertificationLevel")]
        public byte Level { get; set; }
        /// <summary> 个性签名 </summary>
        public string Signature { get; set; }
        /// <summary> 性别 </summary>
        public byte? Gender { get; set; }

        [JsonIgnore]
        public DateTime? ExpireTime { get; set; }

        [JsonIgnore]
        public string RoleDesc
        {
            get
            {
                var roleDesc = "游客";
                if ((Role & (byte)UserRole.Student) > 0)
                {
                    roleDesc = "学生";
                    //                    if (!string.IsNullOrWhiteSpace(StudentNum))
                    //                    {
                    //                        roleDesc += "[" + StudentNum + "]";
                    //                    }
                }
                else if ((Role & (byte)UserRole.Teacher) > 0)
                {
                    roleDesc = SubjectName + "教师";
                }
                else if ((Role & (byte)UserRole.Parents) > 0)
                {
                    roleDesc = "家长";
                }
                return roleDesc;
            }
        }

        /// <summary> 是否是教师 </summary>
        public bool IsTeacher()
        {
            return (Role & (byte)UserRole.Teacher) > 0;
        }

        /// <summary> 是否是学生 </summary>
        public bool IsStudent()
        {
            return (Role & (byte)UserRole.Student) > 0;
        }

        /// <summary> 是否是家长 </summary>
        public bool IsParents()
        {
            return (Role & (byte)UserRole.Parents) > 0;
        }

        /// <summary> 当前角色 </summary>
        /// <returns></returns>
        public UserRole CurrentRole()
        {
            if (IsTeacher())
                return UserRole.Teacher;
            if (IsStudent())
                return UserRole.Student;
            return IsParents() ? UserRole.Parents : UserRole.Caird;
        }
    }
}
