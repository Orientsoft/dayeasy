using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary>
    /// 阅卷结果Result
    /// </summary>
    [AutoMap(typeof(TP_MarkingResult))]
    public class MarkingResultDto : DDto
    {
        public string Id { get; set; }
        public string PaperId { get; set; }
        public string Batch { get; set; }
        public long StudentId { get; set; }
        [MapFrom("ClassID")]
        public string GroupId { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsFinished { get; set; }
        public decimal TotalScore { get; set; }
        public string SectionScores { get; set; }
        public int ErrorQuestionCount { get; set; }
    }
}
