using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TB_Topic : DEntity<string>
    {
        [Key]
        [Column("ID")]
        [StringLength(32)]
        public override string Id { get; set; }

        public int ClassType { get; set; }

        [Required]
        [StringLength(32)]
        public string GroupId { get; set; }

        [StringLength(256)]
        public string Title { get; set; }

        public string Content { get; set; }

        [StringLength(128)]
        public string Tags { get; set; }

        public long AddedBy { get; set; }

        public DateTime AddedAt { get; set; }

        public byte Status { get; set; }

        public int ReadNum { get; set; }

        public int ReplyNum { get; set; }

        public int PraiseNum { get; set; }

        public DateTime? LastRepliedAt { get; set; }

        public long? LastRepliedBy { get; set; }

        public bool HasVote { get; set; }
        public int State { get; set; }
    }
}
