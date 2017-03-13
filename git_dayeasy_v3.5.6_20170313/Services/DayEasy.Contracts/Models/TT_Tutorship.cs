using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TT_Tutorship : DEntity<string>
    {
        [Key]
        [Column("TutorId")]
        public override string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Knowledges { get; set; }
        public int Grade { get; set; }
        public byte Difficulty { get; set; }
        public int SubjectId { get; set; }
        public string Tags { get; set; }
        public string Profile { get; set; }
        public string Author { get; set; }
        public byte Status { get; set; }
        public int UseCount { get; set; }
        public Nullable<int> CommentCount { get; set; }
        public Nullable<decimal> Score { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
    }
}
