using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_LearningMemo : DEntity<string>
    {
        [Key]
        [Column("MemoId")]
        public override string Id { get; set; }
        [NotMapped]
        public string MemoId { get { return Id; } }
        public string Title { get; set; }
        public string Words { get; set; }
        public string Videos { get; set; }
        public string Pictures { get; set; }
        public string Tags { get; set; }
        public byte MemoType { get; set; }
        public long UserId { get; set; }
        public System.DateTime AddedAt { get; set; }
        public byte Status { get; set; }
        public int SubjectId { get; set; }
    }
}
