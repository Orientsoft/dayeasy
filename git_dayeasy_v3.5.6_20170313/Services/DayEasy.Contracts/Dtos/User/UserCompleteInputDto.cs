using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 用户完善信息实体类 </summary>
    public class UserCompleteInputDto : DDto
    {
        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 真实姓名 </summary>
        public string Name { get; set; }

        /// <summary> 性别 </summary>
        public int Gender { get; set; }

        /// <summary> 当前机构 </summary>
        public string AgencyId { get; set; }

        /// <summary> 开始时间 </summary>
        public DateTime Start { get; set; }
    }
}
