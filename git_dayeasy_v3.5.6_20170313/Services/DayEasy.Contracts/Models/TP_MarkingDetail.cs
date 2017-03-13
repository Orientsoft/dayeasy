using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_MarkingDetail : DEntity<string>
    {
        [Key]
        [Column("DetailID")]
        public override string Id { get; set; }
        [ForeignKey("TP_MarkingResult")]
        public string MarkingID { get; set; }
        public string PaperID { get; set; }
        public string Batch { get; set; }
        public long StudentID { get; set; }
        public string QuestionID { get; set; }
        public string SmallQID { get; set; }
        public string AnswerIDs { get; set; }
        public string AnswerContent { get; set; }
        public string AnswerImages { get; set; }
        public System.DateTime AnswerTime { get; set; }
        public decimal Score { get; set; }
        public bool IsFinished { get; set; }
        public Nullable<bool> IsCorrect { get; set; }
        public decimal CurrentScore { get; set; }
        public Nullable<long> MarkingBy { get; set; }
        public Nullable<DateTime> MarkingAt { get; set; }

        public virtual TP_MarkingResult TP_MarkingResult { get; set; }
    }
}
