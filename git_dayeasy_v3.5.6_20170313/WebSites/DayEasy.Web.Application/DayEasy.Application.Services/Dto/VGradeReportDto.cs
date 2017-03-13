using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Application.Services.Dto
{
    /// <summary> 年级报表 </summary>
    public class VGradeReportDto : DDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsUnion { get; set; }
        public DateTime Time { get; set; }
    }
}
