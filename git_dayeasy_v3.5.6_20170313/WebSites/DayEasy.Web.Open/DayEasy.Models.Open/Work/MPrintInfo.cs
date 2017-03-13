
using System;

namespace DayEasy.Models.Open.Work
{
    /// <summary> 套打列表信息 </summary>
    public class MPrintInfo : DDto
    {
        /// <summary> 批次号 </summary>
        public string Batch { get; set; }

        /// <summary> 圈子ID </summary>
        public string GroupId { get; set; }

        /// <summary> 圈子名称 </summary>
        public string GroupName { get; set; }

        /// <summary> 批阅人数 </summary>
        public int MarkingCount { get; set; }

        /// <summary> 批阅状态 </summary>
        public byte MarkingStatus { get; set; }

        public DateTime MarkingDate { get;set; }
        /// <summary> 阅卷时间 </summary>
        public long MarkingTime { get; set; }
        /// <summary> 机构名称 </summary>
        public string AgencyName { get; set; }
    }
}
