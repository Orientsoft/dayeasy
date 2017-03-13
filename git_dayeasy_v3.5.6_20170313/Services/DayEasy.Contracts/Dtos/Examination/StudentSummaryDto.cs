using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos.Statistic;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary>
    /// 学生统计汇总
    /// </summary>
    public class StudentSummaryDto
    {
        /// <summary> 考试ID </summary>
        public string ExaminationId { get; set; }
        /// <summary> 考试名称 </summary>
        public string ExaminationTitle { get; set; }
        /// <summary> 总得分 </summary>
        public decimal TotalScore { get; set; }
        /// <summary> 年级平均分 </summary>
        public decimal AvgGradeScore { get; set; }
        /// <summary> 班级平均分 </summary>
        public decimal AvgClassScore { get; set; }
        /// <summary> 年级排名 </summary>
        public int GradeRank { get; set; }
        /// <summary> 班级排名 </summary>
        public int ClassRank { get; set; }

        public List<StudentSubjectRankDto> Ranks { get; set; }

    }

    public class StudentSubjectBaseDto
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string PaperTitle { get; set; }
        public byte PaperType { get; set; }
        public string SubjectName { get; set; }
    }

    public class StudentSubjectRankDto : StudentSubjectBaseDto
    {
        public decimal TotalScore { get; set; }
        public decimal AScore { get; set; }
        public decimal BScore { get; set; }
        public decimal AvgGradeTotalScore { get; set; }
        public decimal AvgGradeAScore { get; set; }
        public decimal AvgGradeBScore { get; set; }
        public int GradeRank { get; set; }
    }

    public class StudentSubjectSectionDto : StudentSubjectBaseDto
    {
        public int Rank { get; set; }
        public StatisticsScoreDto Section { get; set; }
    }

}
