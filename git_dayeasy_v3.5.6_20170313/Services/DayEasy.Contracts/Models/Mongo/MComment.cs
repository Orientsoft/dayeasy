using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary> 评论实体 </summary>
    public class MongoComment : DEntity<string>
    {
        /// <summary> Mongo内部Id </summary>
        public override string Id { get; set; }
        /// <summary> 资源ID </summary>
        public string SourceId { get; set; }
        /// <summary> 用户ID </summary>
        public long UserId { get; set; }
        /// <summary> 消息内容 </summary>
        public string Message { get; set; }
        /// <summary> 喜欢数量 </summary>
        public int LikeCount { get; set; }
        /// <summary> 不喜欢数量 </summary>
        public int HateCount { get; set; }
        /// <summary> 喜欢列表 </summary>
        public List<long> Likes { get; set; }
        /// <summary> 不喜欢列表 </summary>
        public List<long> Hates { get; set; }

        /// <summary> 父级评论ID </summary>
        public List<string> Parents { get; set; }
        /// <summary> 时间 </summary>
        public DateTime AddedAt { get; set; }

        /// <summary> 楼层 </summary>
        public int Floor { get; set; }

        /// <summary> 状态 </summary>
        public byte Status { get; set; }
    }
}
