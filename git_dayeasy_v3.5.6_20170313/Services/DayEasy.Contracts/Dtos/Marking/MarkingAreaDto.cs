using System.Collections.Generic;
using System.Runtime.Serialization;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    public class MarkingAreaDto : DDto
    {
        public string PaperId { get; set; }
        public int Type { get; set; }
        public string ImageUrl { get; set; }
        public string Areas { get; set; }
        public List<MarkingAreaQuestion> Questions { get; set; }
    }

    public class MarkingAreaQuestion : DDto
    {
        /// <summary> 序号 </summary>
        public string Index { get; set; }

        /// <summary> 题目ID </summary>
        public string Id { get; set; }
        /// <summary> 题型 </summary>
        public int Type { get; set; }

        /// <summary> 是否填空题 </summary>
        public bool T { get { return Type == 7; } }
    }

    [DataContract(Name = "region_spot")]
    public class RegionSpotDto : DDto
    {
        [DataMember(Name = "x", Order = 1)]
        public float X { get; set; }

        [DataMember(Name = "y", Order = 2)]
        public float Y { get; set; }

        [DataMember(Name = "width", Order = 3)]
        public float Width { get; set; }

        [DataMember(Name = "height", Order = 4)]
        public float Height { get; set; }
    }
}
