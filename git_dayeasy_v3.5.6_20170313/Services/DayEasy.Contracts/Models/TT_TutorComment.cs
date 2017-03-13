using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TT_TutorComment : DEntity<string>
    {
        [Key]
        [Column("CommentId")]
        public override string Id { get; set; }
        public string TutorId { get; set; }
        public Nullable<decimal> Score { get; set; }
        public Nullable<byte> ChooseComment { get; set; }
        public string Comment { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
        public byte Status { get; set; }
    }
}
