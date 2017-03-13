using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Topic
{
    public class PubTopicDto : DDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> Tags { get; set; }
        public string GroupId { get; set; }
        public long UserId { get; set; }
        public PubVote PubVote { get; set; }
    }

    public class PubVote : DDto
    {
        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public bool IsSingle { get; set; }
        public bool IsPublic { get; set; }
        public DateTime? FinishedAt { get; set; }
        public List<PubVoteOption> VoteOptions { get; set; }
    }

    public class PubVoteOption : DDto
    {
        public int Sort { get; set; }
        public string OpContent { get; set; }
    }
}
