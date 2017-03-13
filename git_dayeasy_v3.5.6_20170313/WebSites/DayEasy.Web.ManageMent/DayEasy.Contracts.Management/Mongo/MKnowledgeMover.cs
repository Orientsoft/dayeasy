
using System;

namespace DayEasy.Contracts.Management.Mongo
{
    public class MKnowledgeMover
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TargetCode { get; set; }
        public string TargetName { get; set; }
        public byte Status { get; set; }
        public DateTime Creation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishedTime { get; set; }
    }
}
