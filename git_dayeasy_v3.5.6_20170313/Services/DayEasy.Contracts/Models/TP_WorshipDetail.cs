using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_WorshipDetail : DEntity<string>
    {
        [ForeignKey("TP_AnswerShare")]
        public string ShareId { get; set; }
        public string AddedName { get; set; }
        public long AddedBy { get; set; }
        public System.DateTime AddedAt { get; set; }
    
        public virtual TP_AnswerShare TP_AnswerShare { get; set; }
    }
}
