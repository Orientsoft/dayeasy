using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary> 学生分数 </summary>
    public class StudentScoreDto : DDto
    {
        /// <summary> 批次号 </summary>
        public string Batch { get; set; }
        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }
        /// <summary> 排名 </summary>
        public int Rank { get; set; }

        /// <summary> 总分 </summary>
        public decimal Score { get; set; }

        /// <summary> A卷得分 </summary>
        public decimal ScoreA { get; set; }

        /// <summary> B卷得分 </summary>
        public decimal ScoreB { get; set; }

        /// <summary> 构造函数 </summary>
        public StudentScoreDto()
        {
            Rank = -1;
            Score = -1;
            ScoreA = -1;
            ScoreB = -1;
        }
    }
}
