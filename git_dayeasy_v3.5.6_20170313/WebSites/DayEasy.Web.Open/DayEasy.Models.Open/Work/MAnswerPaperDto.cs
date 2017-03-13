using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DayEasy.Models.Open.Work
{
    /// <summary>
    /// 错题统计
    /// </summary>
    public class MAnswerPaperDto : DDto
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string GroupId { get; set; }
        //true：扫描批阅、false：推送试卷
        public bool IsPrint { get; set; }
        //错题统计：题目ID及答错的学生ID列表
        public Dictionary<string, List<long>> Errors { get; set; }
    }

    /// <summary>
    /// 学生答卷-答题明细
    /// </summary>
    public class MMarkingDetailDto : DDto
    {
        public string QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public decimal Score { get; set; }
        //后续可扩展 我的答案；暂时不需要
    }
}
