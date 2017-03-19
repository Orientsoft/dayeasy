using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.ErrorQuestion
{
    /// <summary>
    /// 错题
    /// </summary>
    public class ErrorQuestionDto : DDto
    {
        public string Id { get; set; }
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string PaperTitle { get; set; }
        public string GroupId { get; set; }
        public string Time { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int SourceType { get; set; }
        public byte Status { get; set; }
        public ReasonDto Reason { get; set; }
        public QuestionDto Question { get; set; }
    }
}
