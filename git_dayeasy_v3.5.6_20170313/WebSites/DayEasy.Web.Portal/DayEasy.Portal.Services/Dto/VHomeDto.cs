using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Portal.Services.Dto
{
    public class VHomeDto : DDto
    {
        public long UserId { get; set; }
        public bool IsTeacher { get; set; }
        public string Avatar { get; set; }
        public int MessageCount { get; set; }
        public int TargetCount { get; set; }
        public int UserCount { get; set; }
        public int AgencyCount { get; set; }
        public Dictionary<string, string> HotAgencies { get; set; }
    }
}
