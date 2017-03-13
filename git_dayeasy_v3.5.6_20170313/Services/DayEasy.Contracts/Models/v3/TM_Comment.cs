using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TM_Comment : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string SourceId { get; set; }

        public long UserId { get; set; }

        [Required]
        [StringLength(512)]
        public string Message { get; set; }

        public int LikeCount { get; set; }

        public int HateCount { get; set; }

        [StringLength(2048)]
        public string Likes { get; set; }

        public string Hates { get; set; }

        [StringLength(32)]
        public string ParentId { get; set; }

        public DateTime AddedAt { get; set; }

        public byte Status { get; set; }
    }
}
