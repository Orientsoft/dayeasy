using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DayEasy.Contracts.Dtos.Topic
{
    [AutoMap(typeof(TB_Topic))]
    public class TopicDto : DDto
    {
        public string Id { get; set; }
        public int ClassType { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public List<string> ImgList
        {
            get
            {
                var regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
                var matches = regImg.Matches(Content);
                return (from Match match in matches select match.Groups["imgUrl"].Value).ToList();
            }
        }

        public string Tags { get; set; }

        public List<string> TagList
        {
            get { return string.IsNullOrEmpty(Tags) ? null : Tags.JsonToObject<List<string>>(); }
        }

        public long AddedBy { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        public DateTime AddedAt { get; set; }
        public int ReadNum { get; set; }
        public int ReplyNum { get; set; }
        public int PraiseNum { get; set; }
        public bool HasVote { get; set; }
        public bool IsClose { get; set; }
        public int State { get; set; }

        public List<TopicState> StateList
        {
            get
            {
                return System.Enum.GetValues(typeof(TopicState))
                        .Cast<TopicState>()
                        .Where(state => ((byte)state & State) > 0)
                        .ToList();
            }
        }
        public VoteDto VoteDetail { get; set; }

        /// <summary>
        /// 是否已赞
        /// </summary>
        public bool hadPraised { get; set; }
    }

    /// <summary>
    /// 投票
    /// </summary>
    [AutoMap(typeof(TB_Vote))]
    public class VoteDto : DDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public bool IsSingleSelection { get; set; }
        public bool IsPublic { get; set; }
        public DateTime? FinishedAt { get; set; }
        public List<VoteOptionDto> VoteOptions { get; set; }
        /// <summary>
        /// 是否已经投票
        /// </summary>
        public bool HadVoted { get; set; }
    }

    [AutoMap(typeof(TB_VoteOption))]
    public class VoteOptionDto : DDto
    {
        public string Id { get; set; }
        public string OptionContent { get; set; }
        public int Count { get; set; }
        public int Sort { get; set; }
    }
}
