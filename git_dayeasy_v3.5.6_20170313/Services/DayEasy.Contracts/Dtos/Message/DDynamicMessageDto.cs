using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Timing;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 基础动态消息 </summary>
    public class DDynamicMessageDto : DDto
    {
        public string Id { get; set; }

        /// <summary> 动态类型 </summary>
        public int DynamicType { get; set; }

        /// <summary> 点赞数 </summary>
        public int GoodCount { get; set; }

        /// <summary> 评论数 </summary>
        public int CommentCount { get; set; }

        public bool Liked { get; set; }

        /// <summary> 点赞列表 </summary>
        public List<long> Goods { get; set; }

        public int Status { get; set; }

        public long UserId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime SendTime { get; set; }

        public string Time
        {
            get { return SendTime.ShowTime("yyyy-MM-dd HH:mm"); }
        }
    }
}
