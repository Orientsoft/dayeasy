using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary> 大型考试科目得分率 </summary>
    public class MExaminationQuestionScores : DEntity<string>
    {
        /// <summary> Id </summary>
        public override string Id { get; set; }

        /// <summary> 大型考试ID </summary>
        public string ExamId { get; set; }
        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }

        /// <summary> 得分率 </summary>
        public ScoreRateDto Scores { get; set; }
    }
}
