using DayEasy.Core.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace DayEasy.Contracts.Models
{
    /// <summary> 用户机构表 </summary>
    public class TU_UserAgency : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 机构ID </summary>
        [Required]
        [StringLength(32)]
        public string AgencyId { get; set; }

        /// <summary> 学段 </summary>
        public byte Stage { get; set; }

        /// <summary> 开始时间 </summary>
        public DateTime StartTime { get; set; }

        /// <summary> 截至时间 </summary>
        public DateTime? EndTime { get; set; }

        /// <summary> 状态 </summary>
        public byte Status { get; set; }
        /// <summary> 角色 </summary>
        public byte Role { get; set; }
    }
}
