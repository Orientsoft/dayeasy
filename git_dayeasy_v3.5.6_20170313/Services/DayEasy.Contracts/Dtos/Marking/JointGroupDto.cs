using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    [Serializable]
    public class JointGroupDto : DDto
    {
        public string JointBatch { get; set; }
        public string GroupId { get; set; }

        public string GroupName { get; set; }

        public string GroupCode { get; set; }

        public long UserId { get; set; }
        public string UserName { get; set; }

        public DateTime AddedAt { get; set; }

        public List<string> ClassList { get; set; }
    }
}
