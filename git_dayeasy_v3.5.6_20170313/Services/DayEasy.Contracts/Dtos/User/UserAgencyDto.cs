using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary> 用户机构 </summary>
    public class UserAgencyDto : DDto
    {
        /// <summary> Id </summary>
        public string Id { get; set; }
        /// <summary> 机构ID </summary>
        public string AgencyId { get; set; }
        /// <summary> 机构名称 </summary>
        public string AgencyName { get; set; }
        public byte Level { get; set; }
        /// <summary> 机构Logo </summary>
        public string Logo { get; set; }
        /// <summary> 学段 </summary>
        public byte Stage { get; set; }
        /// <summary> 状态 </summary>
        public byte Status { get; set; }
        /// <summary> 开始时间 </summary>
        public DateTime Start { get; set; }
        /// <summary> 截至时间 </summary>
        public DateTime? End { get; set; }
    }
}
