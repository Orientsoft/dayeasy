using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{

    public class TE_BookDetail : DEntity<string>
    {
        [Key]
        [Column("DetailId")]
        public override string Id { get; set; }
        [NotMapped]
        public string DetailId { get { return Id; } }
        public string ResultId { get; set; }
        public long SenderId { get; set; }
        public string QuestionId { get; set; }
        public string SmallQuestionId { get; set; }
        public string AnswerIds { get; set; }
        public string AnswerContent { get; set; }
        public string AnswerImage { get; set; }
        public System.DateTime AnswerTime { get; set; }
        public Nullable<bool> IsCorrect { get; set; }

        public virtual TE_BookResult TE_BookResult { get; set; }
    }
}
