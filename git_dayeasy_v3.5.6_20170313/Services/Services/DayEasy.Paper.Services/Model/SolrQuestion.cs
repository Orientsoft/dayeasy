using System;
using System.Collections.Generic;
using DayEasy.Assistant.Solr;
using SolrNet.Attributes;

namespace DayEasy.Paper.Services.Model
{
    /// <summary>
    /// 题库Solr实体
    /// </summary>
    [SolrCore("question_core")]
    public class SolrQuestion : SolrEntity
    {
        /// <summary>
        /// 题目ID
        /// </summary>
        [SolrUniqueKey("id")]
        public string QuestionId { get; set; }

        /// <summary>
        /// 题目类型
        /// </summary>
        [SolrField("type")]
        public int Type { get; set; }

        /// <summary>
        /// 学段
        /// </summary>
        [SolrField("stage")]
        public int Stage { get; set; }

        /// <summary>
        /// 科目
        /// </summary>
        [SolrField("subject_id")]
        public int SubjectId { get; set; }

        /// <summary>
        /// 出题人
        /// </summary>
        [SolrField("added_by")]
        public long UserId { get; set; }

        /// <summary>
        /// 题干
        /// </summary>
        [SolrField("bodys")]
        public ICollection<string> Bodys { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [SolrField("tags")]
        public ICollection<string> Tags { get; set; }

        /// <summary>
        /// 知识点
        /// </summary>
        [SolrField("points")]
        public ICollection<string> Points { get; set; }

        /// <summary>
        /// 出题时间
        /// </summary>
        [SolrField("added_at")]
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// 分享范围
        /// </summary>
        [SolrField("share_range")]
        public int ShareRange { get; set; }

        /// <summary>
        /// 分享机构
        /// </summary>
        [SolrField("agency_id")]
        public ICollection<string> AgencyId { get; set; }

        /// <summary>
        /// 分享时间
        /// </summary>
        [SolrField("share_time")]
        public DateTime ShareTime { get; set; }

        /// <summary>
        /// 使用次数
        /// </summary>
        [SolrField("used_count")]
        public long UsedCount { get; set; }

        /// <summary>
        /// 答题次数
        /// </summary>
        [SolrField("answer_count")]
        public long AnswerCount { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        [SolrField("error_count")]
        public long ErrorCount { get; set; }

        /// <summary>
        /// 错误率
        /// </summary>
        [SolrField("error_rate")]
        public double ErrorRate
        {
            get
            {
                if (AnswerCount <= 0) return 0;
                return ErrorCount / (double)AnswerCount;
            }
        }

        /// <summary>
        /// 难度系数
        /// </summary>
        [SolrField("difficulty")]
        public double Difficulty { get; set; }
    }
}
