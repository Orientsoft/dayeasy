using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 选修批次 </summary>
    public class TS_ElectiveBatch : DEntity<string>
    {
        [Key]
        [Column("Batch")]
        [StringLength(32)]
        public override string Id { get; set; }
        /// <summary> 标题 </summary>
        [Required]
        [StringLength(32)]
        public string Title { get; set; }
        /// <summary> 机构ID </summary>
        [Required]
        [StringLength(32)]
        public string AgencyId { get; set; }
        /// <summary> 状态 </summary>
        public byte Status { get; set; }
        /// <summary> 创建时间 </summary>
        public DateTime CreationTime { get; set; }
        /// <summary> 创建人 </summary>
        public long CreatorId { get; set; }

    }
}
