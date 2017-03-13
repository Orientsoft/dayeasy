using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Examination
{
    public class UnionExamDto : DDto
    {
        public string Batch { get; set; }
        public DateTime Time { get; set; }
        public List<DExamDto> Exams { get; set; }
    }
}
