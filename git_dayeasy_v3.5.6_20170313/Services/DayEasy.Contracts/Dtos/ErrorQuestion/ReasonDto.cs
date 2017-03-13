using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility;
using DayEasy.Utility.Timing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.ErrorQuestion
{
    /// <summary>
    /// 错因分析
    /// </summary>
    [AutoMap(typeof(TP_ErrorReason))]
    public class ReasonDto : DDto
    {
        public string Id { get; set; }
        public string ErrorId { get; set; }

        [MapFrom("CommentCount")]
        public int Count { get; set; }

        [MapFrom("Tags")]
        public string Tag { get; set; }

        public List<NameDto> TagList { get; set; }

        public string Content { get; set; }

        public long StudentId { get; set; }

        [JsonIgnore]
        public DateTime AddedAt { get; set; }

        public string Time
        {
            get { return Utils.GetTime(AddedAt); }
        }

        public bool IsFinished { get; set; }
    }

    [AutoMap(typeof(TP_ErrorReason))]
    public class ReasonExtDto : ReasonDto
    {
        public string UserName { get; set; }
        public string HeadPic { get; set; }
    }

    /// <summary>
    /// 错因分析评论
    /// </summary>
    [AutoMapFrom(typeof(TP_ErrorReasonComment))]
    public class ReasonCommentDto : DDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        [MapFrom("AddedBy")]
        public long UserId { get; set; }
        public int UserRole { get; set; }
        public string UserName { get; set; }
        public string Head { get; set; }
        [JsonIgnore]
        public DateTime AddedAt { get; set; }
        public string Time { get { return AddedAt.ShowTime(); } }
        public string ParentId { get; set; }
        public string ParentName { get; set; }
        public int DetailCount { get; set; }
        public List<ReasonCommentDto> Details { get; set; }
    }
}
