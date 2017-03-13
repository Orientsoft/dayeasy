using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TQ_Answer : DEntity<string>
    {
        [Key]
        [Column("AnswerID")]
        public override string Id { get; set; }
        public string QuestionID { get; set; }
        public string QContent { get; set; }
        public string QImages { get; set; }
        public int Sort { get; set; }
        public bool IsCorrect { get; set; }
    }
}
