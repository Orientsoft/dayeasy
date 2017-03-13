
using DayEasy.Assistant.Solr;
using SolrNet;

namespace DayEasy.Paper.Services.Model
{
    /// <summary> 排序类型 </summary>
    public enum QuestionOrderType : byte
    {
        /// <summary> 出题时间倒序 </summary>
        [DSolrOrder("added_at", Order.DESC)]
        AddedAtDesc = 1,

        /// <summary> 出题时间顺序 </summary>
        [DSolrOrder("added_at")]
        AddedAt = 2,

        /// <summary> 分享时间倒序 </summary>
        [DSolrOrder("share_time", Order.DESC)]
        ShareTimeDesc = 3,

        /// <summary> 分享时间顺序 </summary>
        [DSolrOrder("share_time")]
        ShareTime = 4,

        /// <summary> 错误次数倒序 </summary>
        [DSolrOrder("error_count", Order.DESC)]
        ErrorCountDesc = 5,

        /// <summary> 错误次数顺序 </summary>
        [DSolrOrder("error_count")]
        ErrorCount = 6,

        /// <summary> 答题次数倒序 </summary>
        [DSolrOrder("answer_count", Order.DESC)]
        AnswerCountDesc = 7,

        /// <summary> 答题次数顺序 </summary>
        [DSolrOrder("answer_count")]
        AnswerCount = 8,

        /// <summary> 难度系数 </summary>
        [DSolrOrder("difficulty")]
        Difficulty = 9,

        /// <summary> 难度系数倒序 </summary>
        [DSolrOrder("difficulty", Order.DESC)]
        DifficultyDesc = 10,

        /// <summary> 错误率 </summary>
        [DSolrOrder("error_rate")]
        ErrorRate = 11,

        /// <summary> 错误率倒序 </summary>
        [DSolrOrder("error_rate", Order.DESC)]
        ErrorRateDesc = 12,

        /// <summary> 使用次数 </summary>
        [DSolrOrder("used_count")]
        UsedCount = 13,

        /// <summary> 使用次数倒序 </summary>
        [DSolrOrder("used_count", Order.DESC)]
        UsedCountDesc = 14,

        /// <summary> 随机排序 </summary>
        Random = 101
    }
}
