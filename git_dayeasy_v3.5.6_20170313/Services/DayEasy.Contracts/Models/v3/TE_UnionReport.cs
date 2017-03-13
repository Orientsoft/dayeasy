using DayEasy.Core.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DayEasy.Contracts.Models
{
    public class TE_UnionReport : DEntity<string>
    {
        /// <summary>
        /// 关联批次
        /// </summary>
        [Key]
        [Column("Batch")]
        [StringLength(32)]
        public override string Id { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddedAt { get; set; }
        /// <summary>
        /// 添加人
        /// </summary>
        public long AddedBy { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public byte Status { get; set; }
    }
}
