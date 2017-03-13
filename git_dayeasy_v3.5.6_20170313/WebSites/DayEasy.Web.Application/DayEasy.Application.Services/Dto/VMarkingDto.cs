using DayEasy.Contracts.Dtos.Group;
using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Application.Services.Dto
{
    /// <summary> 批阅中心页面实体 </summary>
    public class VMarkingDto : DDto
    {
        /// <summary> 发布批次 or 协同批次 </summary>
        public string Batch { get; set; }

        public string PaperId { get; set; }
        public string PaperTitle { get; set; }
        public byte PaperType { get; set; }
        public byte Status { get; set; }
        /// <summary> A卷份数 </summary>
        public int ACount { get; set; }
        /// <summary> B卷份数 </summary>
        public int BCount { get; set; }
        /// <summary> 班级圈 or 同事圈 </summary>
        public DGroupDto Group { get; set; }
        public DateTime Time { get; set; }
        /// <summary> 是否是协同 </summary>
        public bool IsJoint { get; set; }

        /// <summary> 是否已分配 </summary>
        public bool Alloted { get; set; }
        public bool IsOwner { get; set; }
    }
}
