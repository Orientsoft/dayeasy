using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary>
    /// 统计信息--板块成绩信息
    /// </summary>
    public class ReportSectionScoresDto : DDto
    {
        public decimal Ah { get; set; }
        public decimal Al { get; set; }
        public decimal AAv { get; set; }
        public decimal Bh { get; set; }
        public decimal Bl { get; set; }
        public decimal BAv { get; set; }
    }

    /// <summary>
    /// 统计信息--分数分段信息
    /// </summary>
    public class ScoreGroupsDto : DDto
    {
        /// <summary>
        /// 分数段信息
        /// </summary>
        public string ScoreInfo { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 用于数据展示的容器
    /// </summary>
    public class SeriesDto : DDto
    {
        public string name { get; set; }
        public List<PointDto> data { get; set; }
    }

    /// <summary>
    /// 用于数据展示的数据点容器
    /// </summary>
    public class PointDto : DDto
    {
        public double x { get; set; }
        public decimal y { get; set; }
        public PointInfoDto PointInfo { get; set; }
    }

    /// <summary>
    /// 用于数据展示的数据点信息容器
    /// </summary>
    public class PointInfoDto : DDto
    {
        public string PId { get; set; }
        public string Batch { get; set; }
        public int PType { get; set; }
        public string PName { get; set; }
        public decimal Average { get; set; }
        public decimal HScore { get; set; }
        public decimal LScore { get; set; }
        public decimal AAverage { get; set; }
        public decimal BAverage { get; set; }
        public List<ScoreGroupsDto> SGroup { get; set; }
        public bool IsSelf { get; set; }
    }

    /// <summary>
    /// 学生排名数据Dto
    /// </summary>
    public class StudentRankDataDto : DDto
    {
        public List<UsageDto> UsageList { get; set; } 
        public List<StudentRankDto> StudentRankList { get; set; } 
    }

    /// <summary>
    /// 
    /// </summary>
    public class UsageDto : DDto
    {
        public string Batch { get; set; }
        public string SourceId { get; set; }
        public DateTime AddedTime { get; set; }
        public string PaperName { get; set; }
    }

    /// <summary>
    /// 学生排名
    /// </summary>
    public class StudentRankDto : DDto
    {
        public long StudentId { get; set; }
        public string Batch { get; set; }
        public int Rank { get; set; }
    }

    /// <summary>
    /// 用于数据展示的容器
    /// </summary>
    public class StudentSeriesDto : DDto
    {
        public string name { get; set; }
        public List<StudentPointDto> data { get; set; }
    }

    /// <summary>
    /// 用于学生端数据展示的容器
    /// </summary>
    public class StudentPointDto : DDto
    {
        public double x { get; set; }
        public int y { get; set; }
        public decimal Score { get; set; }
        public decimal AScore { get; set; }
        public decimal BScore { get; set; }
        public PointInfoDto PointInfo { get; set; }
    }
}
