
namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 学科分析计算参数 </summary>
    public class SubjectAnalysisInputDto : AnalysisInputDto
    {
        /// <summary> 考试科目Id </summary>
        public string ExamSubjectId { get; set; }

        public SubjectAnalysisInputDto()
        {
            KeyScore = 100;
        }
    }
}
