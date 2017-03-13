
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary> 试卷答案传输对象 </summary>
    [AutoMap(typeof(TP_PaperAnswer))]
    public class PaperAnswerDto:DDto
    {
        public string QuestionId { get; set; }
        public string SmallQuId { get; set; }
        public string AnswerContent { get; set; }
    }
}
