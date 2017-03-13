using System;
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Statistic
{

    #region 教师作业中心统计详情模型

    /// <summary>
    /// 题目统计
    /// </summary>
    public class StatisticsPaperDto : DDto
    {
        public string PaperId { get; set; }
        public string PaperName { get; set; }
        /// <summary> 是否为推送试卷 </summary>
        public bool IsPush { get; set; }

        /// <summary> 圈子ID </summary>
        public string GroupId { get; set; }
        public List<StatisticsPaperSectionDto> Sections { get; set; }
    }

    /// <summary>
    /// 题目统计 - 版块
    /// </summary>
    public class StatisticsPaperSectionDto : DDto
    {
        /// <summary> 版块序号 </summary>
        public int Sort { get; set; }

        /// <summary> 版块类型：A卷、B卷、常规卷 </summary>
        public byte SectionType { get; set; }

        /// <summary> 版块描述 </summary>
        public string Desc { get; set; }

        /// <summary> 问题列表 </summary>
        public List<StatisticsPaperQuestionDto> Questions { get; set; }
    }

    /// <summary>
    /// 题目统计 - 基类
    /// </summary>
    public class StatisticsPaperQuestionBase : DDto
    {
        //序号
        public int Sort { get; set; }
        //问题ID
        public string QuestionId { get; set; }
        //题干
        public string Body { get; set; }
    }

    /// <summary>
    /// 题目统计 - 大题
    /// </summary>
    [Serializable]
    public class StatisticsPaperQuestionDto : StatisticsPaperQuestionBase
    {
        //是否为客观题
        public bool IsObjective { get; set; }

        //小问
        public List<StatisticsPaperQuestionBase> SmallQuestions { get; set; }

        /// <summary> 答错学生ID </summary>
        public List<long> ErrorStudents { get; set; }
        public decimal Score { get; set; }
    }

    /// <summary>
    /// 答题统计
    /// </summary>
    [Serializable]
    public class StatisticsQuestionDto : DDto
    {
        //参考答案
        public string SystemAnswer { get; set; }

        //学生作答选项分组
        public List<StatisticsQuestionAnswerDto> Answers { get; set; }
    }

    /// <summary>
    /// 答题统计 - 详细
    /// </summary>
    public class StatisticsQuestionAnswerDto : DDto
    {
        //选项
        public string Name { get; set; }

        //比例
        public double Y { get; set; }

        //选择此选项的学生ID
        public List<long> Students { get; set; }
    }

    #endregion

    #region 分数排名统计

    /// <summary>
    /// 学生排名信息
    /// </summary>
    public class StudentRankInfoDto : DDto
    {
        public string Id { get; set; }
        public int Rank { get; set; }
        public int? LastRank { get; set; }
        public long StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentNum { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string AgencyId { get; set; }
        public string AgencyName { get; set; }
        public int ErrorQuestionCount { get; set; }
        public decimal? TotalScore { get; set; }
        public decimal? SectionAScore { get; set; }
        public decimal? SectionBScore { get; set; }
        //关联手机号码：学生、学生家长
        public List<string> Mobiles { get; set; }
    }

    /// <summary>
    /// 分数统计
    /// </summary>
    public class StatisticsScoreDto : DDto
    {
        public string GroupId { get; set; }

        public string GroupName { get; set; }
        public string AgencyId { get; set; }
        public string AgencyName { get; set; }

        //生成该统计的时间，用于区分同一班级多次考试
        public string Time { get; set; }
        //总参考人数
        public int Count { get; set; }
        //及格率
        public decimal PassRate { get; set; }

        public decimal Max { get; set; }

        public decimal Min { get; set; }

        public decimal Avg { get; set; }

        //AB卷最高分、最低分、平均分
        public ReportSectionScoresDto AbAvg { get; set; }

        //总分分数段
        public List<ScoreGroupsDto> ScoreGroupes { get; set; }

        //AB卷分数段
        public List<List<ScoreGroupsDto>> AbScoreGroupes { get; set; }
    }

    #endregion

    #region 考试概况学生排名

    /// <summary>
    /// 考试概况 - 排名分析
    /// </summary>
    public class SurveyAnalysis : DDto
    {
        public bool IsAb { get; set; }
        public List<DUserDto> TopTen { get; set; }

        public List<DUserDto> LastTen { get; set; }

        public List<SurveyTrendDto> Progress { get; set; }

        public List<SurveyTrendDto> BackSlide { get; set; }

        public List<DUserDto> Fails { get; set; }

        public List<DUserDto> FailsA { get; set; }

        public List<DUserDto> UnSubmits { get; set; }

        public SurveyAnalysis()
        {
            TopTen = new List<DUserDto>();
            LastTen = new List<DUserDto>();
            Progress = new List<SurveyTrendDto>();
            BackSlide = new List<SurveyTrendDto>();
            Fails = new List<DUserDto>();
            UnSubmits = new List<DUserDto>();
        }
    }

    public class SurveyTrendDto : DDto
    {
        public DUserDto UserBase { get; set; }
        public int Trend { get; set; }
    }

    #endregion
}
