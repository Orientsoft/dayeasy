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

        /// <summary> Ⱥ��Ƭ </summary>
        [StringLength(16)]
        public string BusinessCard { get; set; }

        /// <summary> ��Ա��ɫ </summary>
        public byte MemberRole { get; set; }

        public byte Status { get; set; }

        public DateTime AddedAt { get; set; }

        /// <summary> ������ʱ�� </summary>
        public DateTime? LastUpDateTime { get; set; }
        /// <summary> ������ </summary>
        public int? TopicNum { get; set; }
    }
}
