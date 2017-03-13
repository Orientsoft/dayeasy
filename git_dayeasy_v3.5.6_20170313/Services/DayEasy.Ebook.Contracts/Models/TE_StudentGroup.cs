using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_StudentGroup : DEntity<string>
    {
        [Key]
        [Column("GroupId")]
        public override string Id { get; set; }
        [NotMapped]
        public string GroupId { get { return Id; } }
        public long TeacherId { get; set; }
        public string GroupName { get; set; }
        public DateTime CreationTime { get; set; }
        public int StudentCount { get; set; }
        public int RecommendCount { get; set; }
        public byte Status { get; set; }
        public Nullable<DateTime> LastRecommendTime { get; set; }
        public int UnViewCount { get; set; }
        public string Profile { get; set; }
    }
}
