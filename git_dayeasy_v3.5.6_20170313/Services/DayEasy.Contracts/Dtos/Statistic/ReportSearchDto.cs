using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary>
    /// 班级分数统计查询dto
    /// </summary>
    public class SearchClassScoresDto : DDto
    {
        public long UserId { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public int SubjectId { get; set; }
        public string GradeYear { get; set; }
    }

    /// <summary>
    /// 查询学生成绩排名dto
    /// </summary>
    public class SearchStudentRankDto : DDto
    {
        public long StudentId { get; set; }
        public long TeacherId { get; set; }
        public string StartTimeStr { get; set; }
        public string EndTimeStr { get; set; }
        public int SubjectId { get; set; }

    }
}
