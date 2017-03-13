using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_MemoReview : DEntity<string>
    {
        [Key]
        [Column("ReviewId")]
        public override string Id { get; set; }
        [NotMapped]
        public string ReviewId { get { return Id; } }
        public string Batch { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public byte ReviewType { get; set; }
        public string ParentId { get; set; }
        public Nullable<int> LikeCount { get; set; }
        public string LikeDetail { get; set; }
        public System.DateTime AddedAt { get; set; }
        public bool IsView { get; set; }
    }
}
