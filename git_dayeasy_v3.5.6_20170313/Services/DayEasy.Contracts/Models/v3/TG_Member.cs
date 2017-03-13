using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TG_Member : DEntity<string>
    {
        [Key]
        [Column("RelationId")]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string GroupId { get; set; }

        public long MemberId { get; set; }

        /// <summary> 群名片 </summary>
        [StringLength(16)]
        public string BusinessCard { get; set; }

        /// <summary> 成员角色 </summary>
        public byte MemberRole { get; set; }

        public byte Status { get; set; }

        public DateTime AddedAt { get; set; }

        /// <summary> 最后更新时间 </summary>
        public DateTime? LastUpDateTime { get; set; }
        /// <summary> 发帖数 </summary>
        public int? TopicNum { get; set; }
    }
}
