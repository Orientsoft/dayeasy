using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace DayEasy.Contracts.Management.Dto
{
    public class AsyncMissionDto : DDto
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public byte Status { get; set; }
        public int Priority { get; set; }
        public string Param { get; set; }
        public int FailCount { get; set; }
        public string Message { get; set; }
        public Dictionary<DateTime, string> Trys { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public string Logs { get; set; }
        public TimeSpan? UseTime
        {
            get
            {
                if (!StartTime.HasValue || !FinishedTime.HasValue)
                    return null;
                return FinishedTime.Value - StartTime.Value;
            }
        }

        public UserDto Creator { get; set; }
    }
}
