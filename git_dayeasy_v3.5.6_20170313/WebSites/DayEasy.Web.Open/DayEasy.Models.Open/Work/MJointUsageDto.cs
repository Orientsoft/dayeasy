
using System.Collections.Generic;

namespace DayEasy.Models.Open.Work
{
    public class MJointUsageDto : DDto
    {
        public string JointBatch { get; set; }
        public string GroupId { get; set; }

        public string GroupName { get; set; }

        public string GroupCode { get; set; }

        public long UserId { get; set; }
        public string UserName { get; set; }

        public List<string> ClassList { get; set; }
    }
}
