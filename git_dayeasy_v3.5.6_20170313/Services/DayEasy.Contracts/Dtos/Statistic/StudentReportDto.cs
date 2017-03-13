using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary> 学生成绩报表 </summary>
    public class StudentReportDto : DDto
    {
        /// <summary> 满分 </summary>
        public decimal TotalScore { get; set; }
        /// <summary> 班级排名 </summary>
        public ReportRankDto ClassRank { get; set; }

        /// <summary> 年级排名 </summary>
        public ReportRankDto GradeRank { get; set; }

        /// <summary> 分数段统计 </summary>
        public List<ReportSegmentDto> Segments { get; set; }

        /// <summary> 排名详情 </summary>
        public List<ReportRankDetailDto> Ranks { get; set; }

        /// <summary> 构造函数 </summary>
        public StudentReportDto()
        {
            ClassRank = new ReportRankDto();
            GradeRank = new ReportRankDto();
            Segments = new List<ReportSegmentDto>();
            Ranks = new List<ReportRankDetailDto>();
        }
    }

    /// <summary> 学生排名 </summary>
    public class ReportRankDto : DDto
    {
        /// <summary> 排名 </summary>
        public int Rank { get; set; }

        /// <summary> 击败的百分比 </summary>
        public decimal Percent { get; set; }

        /// <summary> 排名变化 </summary>
        public int? Change { get; set; }

        /// <summary> 平均分 </summary>
        public decimal Average { get; set; }

        /// <summary> A卷平均分 </summary>
        public decimal AverageA { get; set; }

        /// <summary> B卷平均分 </summary>
        public decimal AverageB { get; set; }

        /// <summary> 构造函数 </summary>
        public ReportRankDto()
        {
            Rank = -1;
            Percent = -1;
            Change = null;
            Average = -1;
            AverageA = -1;
            AverageB = -1;
        }
    }

    /// <summary> 排名详情 </summary>
    public class ReportRankDetailDto : DDto
    {
        /// <summary> 学生Id </summary>
        public long Id { get; set; }

        /// <summary> 排名 </summary>
        public int Rank { get; set; }
        /// <summary> 分数 </summary>
        public decimal Score { get; set; }
        /// <summary> 是否是学生本人 </summary>
        public bool IsMine { get; set; }
    }

    /// <summary> 分数段统计 </summary>
    public class ReportSegmentDto : DDto
    {
        /// <summary> 分数段 </summary>
        public string Segment { get; set; }

        /// <summary> 人数 </summary>
        public int Count { get; set; }

        /// <summary> 是否是我所在的分数段 </summary>
        public bool ContainsMe { get; set; }
    }
}
