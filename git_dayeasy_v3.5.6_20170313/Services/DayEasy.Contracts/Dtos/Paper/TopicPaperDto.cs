
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary> 试卷选填对象 </summary>
    public class TopicPaperDto : DDto
    {
        public string PaperId { get; set; }
        public string PaperName { get; set; }
        public int QuestionCount { get; set; }
    }
}
