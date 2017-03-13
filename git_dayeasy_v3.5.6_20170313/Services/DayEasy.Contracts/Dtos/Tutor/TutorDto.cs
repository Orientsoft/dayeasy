using AutoMapper;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Tutor
{
    [AutoMap(typeof(TT_Tutorship))]
    public class TutorDto : DDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int Grade { get; set; }
        public byte Difficulty { get; set; }
        public int SubjectId { get; set; }
        [IgnoreMap]
        public Dictionary<string, string> Knowledges { get; set; }
        [IgnoreMap]
        public List<string> Tags { get; set; }
        public string Profile { get; set; }
        public string Author { get; set; }
        public byte Status { get; set; }
        public int UseCount { get; set; }
        public int? CommentCount { get; set; }
        public decimal? Score { get; set; }
        public DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
    }
}
