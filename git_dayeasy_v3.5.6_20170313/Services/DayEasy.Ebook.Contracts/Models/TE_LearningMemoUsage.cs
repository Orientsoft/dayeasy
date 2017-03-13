using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{

    public class TE_LearningMemoUsage : DEntity<string>
    {
        [Key]
        [Column("Batch")]
        public override string Id { get; set; }
        [NotMapped]
        public string Batch { get { return Id; } }
        public string MemoId { get; set; }
        public string GroupId { get; set; }
        public int LikeCount { get; set; }
        public int BadCount { get; set; }
        public int ReviewCount { get; set; }
        public string LikeDetail { get; set; }
        public byte Status { get; set; }
        public int SubjectId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
        public Nullable<DateTime> LastUpdateTime { get; set; }
        public int UnViewCount { get; set; }
    }
}
