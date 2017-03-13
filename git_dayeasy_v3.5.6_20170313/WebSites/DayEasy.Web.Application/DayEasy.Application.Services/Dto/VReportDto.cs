using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Application.Services.Dto
{
    /// <summary> 报表中心 - 班级统计页面实体 </summary>
    public class VReportDto : DDto
    {
        public DateTime Date { get; set; }
        public string PaperId { get; set; }
        public string PaperTitle { get; set; }
        public PaperType PaperType { get; set; }
        public PublishType PublishType { get; set; }
        public DGroupDto Group { get; set; }
        public string Batch { get; set; }
        public bool IsJoint { get; set; }
        public string JointBatch { get; set; }
    }
}
