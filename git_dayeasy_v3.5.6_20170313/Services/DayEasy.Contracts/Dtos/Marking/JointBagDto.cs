using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary> 协同包 </summary>
    public class JointBagDto : DDto
    {
        public Dictionary<string, string> QuestionSorts { get; set; }
        public DRegion Region { get; set; }
    }
}
