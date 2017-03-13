using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public partial class TB_Reply : DEntity<string>
    {
        [Key]
        [Column("ID")]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string TopicId { get; set; }

        [Required]
        [StringLength(32)]
        public string ParentId { get; set; }

        [Required]
        public string ReplyContent { get; set; }

        public long ReplyBy { get; set; }

        public long ReplyTo { get; set; }

        public DateTime ReplyAt { get; set; }

        public byte Status { get; set; }
    }
}
