using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary>
    /// 阅卷结束  查询数据库统计
    /// </summary>
    public class SearchDetailDataDto : DDto
    {
        public long StudentID { get; set; }
        public decimal Score { get; set; }
        public int ErrorCount { get; set; }
        public decimal ScoreA { get; set; }
        public decimal ScoreB { get; set; }
    }

    /// <summary>
    /// 阅卷详情  查询
    /// </summary>
    public class MarkDetailDto : DDto
    {
        public long StudentId { get; set; }
        public string QuestionId { get; set; }
        public bool? IsCorrect { get; set; }
    }

    /// <summary>
    /// 查询问题详情
    /// </summary>
    public class QuestionDetailDto : DDto
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public string KpIds { get; set; }
    }

    /// <summary>
    /// 分享答案Dto
    /// </summary>
    public class ShareAnswerDto : DDto
    {
        public string ShareId { get; set; }
        public bool? IsCorrect { get; set; }
        public long StudentId { get; set; }
    }

    /// <summary>
    /// 知识点统计
    /// </summary>
    public class KpStatisticsDto : DDto
    {
        public KpStatisticsDto()
        {
            UpdateTeacherKpStatistic = new List<TS_TeacherKpStatistic>();
            AddTeacherKpStatistic = new List<TS_TeacherKpStatistic>();
            UpdateStudentKpStatistic = new List<TS_StudentKpStatistic>();
            AddStudentKpStatistic = new List<TS_StudentKpStatistic>();
        }

        public List<TS_TeacherKpStatistic> UpdateTeacherKpStatistic { get; set; }
        public List<TS_TeacherKpStatistic> AddTeacherKpStatistic { get; set; }
        public List<TS_StudentKpStatistic> UpdateStudentKpStatistic { get; set; }
        public List<TS_StudentKpStatistic> AddStudentKpStatistic { get; set; }
    }
}
