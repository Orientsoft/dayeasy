using System;
using System.Collections.Generic;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Timing;

namespace DayEasy.Contracts.Dtos.Message
{
    /// <summary> 评论dto </summary>
    [AutoMapFrom(typeof(MongoComment))]
    public class CommentDto : DDto
    {
        public string Id { get; set; }

        /// <summary> 回复某人 </summary>
        public string ParentId { get; set; }

        /// <summary> 回复列表 </summary>
        public List<CommentDto> Replys { get; set; }

        /// <summary> 评论人 </summary>
        public long UserId { get; set; }

        /// <summary> 评论内容 </summary>
        public string Message { get; set; }

        //        [MapFrom("LikeCount")]
        //        public int Like { get; set; }
        //        [MapFrom("HateCount")]
        //        public int Hate { get; set; }
        //
        //        public List<long> Likes { get; set; }
        //
        //        public List<long> Hates { get; set; }

        [MapFrom("AddedAt")]
        public DateTime CreateTime { get; set; }

        public string Time { get { return CreateTime.ShowTime(); } }

        /// <summary> 楼层 </summary>
        public int Floor { get; set; }
    }
}
