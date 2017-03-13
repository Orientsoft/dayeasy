using System;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    [AutoMap(typeof(TP_JointMarking))]
    public class JointMarkingDto : DDto
    {
        [MapFrom("Id")]
        public string Batch { get; set; }
        public string GroupId { get; set; }

        [MapFrom("AddedBy")]
        public long UserId { get; set; }

        public string PaperId { get; set; }
        public int PaperCount { get; set; }

        public byte Status { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime FinishedTime { get; set; }
    }
}
