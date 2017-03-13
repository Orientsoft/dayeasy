using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    public class DExamSubjectDto : DDto
    {
        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }
        /// <summary> 科目 </summary>
        public string Subject { get; set; }
        /// <summary> 试卷类型 </summary>
        public byte PaperType { get; set; }
    }
}
