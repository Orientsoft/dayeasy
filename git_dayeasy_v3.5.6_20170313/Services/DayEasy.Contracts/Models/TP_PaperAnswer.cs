using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_PaperAnswer : DEntity<string>
    {
        public string PaperId { get; set; }
        public string QuestionId { get; set; }
        public string SmallQuId { get; set; }
        public string AnswerContent { get; set; }
    }
}
