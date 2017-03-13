using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 学生成绩排名 </summary>
    public class StudentRankDto : DDto
    {
        /// <summary> A卷分数 </summary>
        public decimal ScoreA { get; set; }

        /// <summary> B卷分数 </summary>
        public decimal ScoreB { get; set; }

        /// <summary> 总分 </summary>
        public decimal Score { get; set; }

        /// <summary> 班级排名 </summary>
        public int ClassRank { get; set; }

        /// <summary> 年级排名 </summary>
        public int Rank { get; set; }
    }
}
