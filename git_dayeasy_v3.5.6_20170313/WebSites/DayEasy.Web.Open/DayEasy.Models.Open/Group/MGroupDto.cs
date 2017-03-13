
using System;
using Newtonsoft.Json;

namespace DayEasy.Models.Open.Group
{
    public class MGroupDto : DDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Logo { get; set; }

        public string GroupBanner { get; set; }
        public byte Type { get; set; }
        public int Count { get; set; }
        public int Capacity { get; set; }
        public string GroupSummary { get; set; }

        public long ManagerId { get; set; }
        [JsonIgnore]
        public DateTime CreationTime { get; set; }

        public long Time { get { return ToLong(CreationTime); } }
        public string AgencyName { get; set; }

        public string Owner { get; set; }

        /// <summary> 是否认证 </summary>
        public bool IsAuth { get; set; }
        public int PendingCount { get; set; }
        public int MessageCount { get; set; }
    }
}
