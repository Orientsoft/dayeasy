
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using System;

namespace DayEasy.Contracts.Management.Dto
{
    public class JointMarkingDto
    {
        public string Id { get; set; }
        public byte Status { get; set; }
        public int PaperACount { get; set; }
        public int PaperBCount { get; set; }
        public int ExceptionCount { get; set; }

        public DGroupDto Group { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string PaperId { get; set; }
        public string PaperNo { get; set; }
        public string PaperTitle { get; set; }
        public UserDto User { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime? FinishedTime { get; set; }
    }
}
