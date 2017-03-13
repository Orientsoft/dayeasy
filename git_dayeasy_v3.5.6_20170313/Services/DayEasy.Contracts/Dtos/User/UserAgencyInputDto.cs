using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 用户机构 </summary>
    public class UserAgencyInputDto : DDto
    {
        /// <summary> 用户ID </summary>
        public long UserId { get; set; }
        /// <summary> 机构ID </summary>
        public string AgencyId { get; set; }
        /// <summary> 机构状态 </summary>
        public byte Status { get; set; }
        /// <summary> 开始时间 </summary>
        public DateTime? Start { get; set; }
        /// <summary> 结束时间 </summary>
        public DateTime? End { get; set; }
    }
}
