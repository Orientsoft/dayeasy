using System;
using System.Collections.Generic;

namespace DayEasy.Models.Open.LearningMemo
{
    public class MemoUsageDto : DDto
    {
        /// <summary> 批次号 </summary>
        public string Batch { get; set; }

        /// <summary> 发布时间 </summary>
        public DateTime PublishTime { get; set; }

        /// <summary> 发布时间(long) </summary>
        public long PublishTimeLong
        {
            get { return ToLong(PublishTime); }
            set { PublishTime = ToDateTime(value); }
        }

        /// <summary> 学生组ID </summary>
        public string GroupId { get; set; }

        /// <summary> 最后更新时间 </summary>
        public DateTime? LastUpdateTime { get; set; }

        /// <summary> 最后更新时间 </summary>
        public long? LastUpdateTimeLong
        {
            get
            {
                if (LastUpdateTime.HasValue)
                    return ToLong(LastUpdateTime.Value);
                return null;
            }
            set
            {
                if (!value.HasValue)
                    LastUpdateTime = null;
                else
                    LastUpdateTime = ToDateTime(value.Value);
            }
        }

        /// <summary> 评论数 </summary>")]
        public int ReviewCount { get; set; }

        /// <summary> 未读评论数 </summary>
        public int UnViewCount { get; set; }

        /// <summary> 喜欢次数 </summary>
        public int LikeCount { get; set; }

        /// <summary> 心碎次数 </summary>
        public int BadCount { get; set; }

        /// <summary> 喜欢/心碎详情 </summary>
        public List<LikeDetailDto> LikeDetail { get; set; }

        /// <summary> 是否已操作 </summary>
        public bool? GoodOrBad { get; set; }
    }
}
