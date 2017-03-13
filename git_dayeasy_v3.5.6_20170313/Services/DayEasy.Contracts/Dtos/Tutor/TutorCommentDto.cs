using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Dtos.Tutor
{
    [AutoMap(typeof(TT_TutorComment))]
    public class TutorCommentDto : DDto
    {
        public string Id { get; set; }
        public string TutorId { get; set; }
        public decimal? Score { get; set; }
        public byte? ChooseComment { get; set; }
        public string Comment { get; set; }
        public DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
        public byte Status { get; set; }
    }
}
