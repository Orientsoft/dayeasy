using System;
using System.Collections.Generic;
using DayEasy.Models.Open.User;
using Newtonsoft.Json;

namespace DayEasy.Models.Open.LearningMemo
{
    public class ReviewDto : DDto
    {
        public MUserBaseDto User { get; set; }

        /// <summary> 内容 </summary>
        public string Content { get; set; }

        /// <summary> 评论类型 </summary>
        public byte ReviewType { get; set; }

        /// <summary> 发布时间 </summary>
        [JsonIgnore]
        public DateTime CreationTime { get; set; }

        /// <summary> 创建时间(long) </summary>
        public long Time
        {
            get { return ToLong(CreationTime); }
            set { CreationTime = ToDateTime(value); }
        }

        /// <summary> 喜欢人次 </summary>
        public int LikeCount { get; set; }

        /// <summary> 喜欢详情 </summary>
        public List<LikeDetailDto> LikeDetail { get; set; }

        /// <summary> 是否已喜欢 </summary>
        public bool IsLiked { get; set; }
    }

    public class UsageReviewDto : ReviewDto
    {
        /// <summary> 批次号 </summary>
        public string Batch { get; set; }

        /// <summary> 回复的评论ID </summary>
        public List<ReviewDto> Replies { get; set; }
    }
}
