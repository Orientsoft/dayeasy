using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    public class AnalysisInputDto : DDto
    {
        /// <summary> 考试ID </summary>
        public string ExamId { get; set; }

        /// <summary> 重点率类型 </summary>
        public byte KeyType { get; set; }

        /// <summary> 重点率值 </summary>
        public decimal KeyScore { get; set; }

        /// <summary> A卷平均分合格分数 </summary>
        public decimal ScoreA { get; set; }

        /// <summary> A卷平均分不合格分数 </summary>
        public decimal UnScoreA { get; set; }

        public AnalysisInputDto()
        {
            KeyScore = 80;
            ScoreA = 60;
            UnScoreA = 40;
        }
    }
}
