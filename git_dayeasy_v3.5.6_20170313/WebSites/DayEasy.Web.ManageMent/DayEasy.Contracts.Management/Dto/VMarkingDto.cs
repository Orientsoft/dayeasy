using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Core.Domain.Entities;
using System;

namespace DayEasy.Contracts.Management.Dto
{
    public class VMarkingInputDto : DPage
    {
        public int SubjectId { get; set; }
        public int PublishType { get; set; }
        public int MarkingStatus { get; set; }
        public string AgencyId { get; set; }
        public string Keyword { get; set; }
        public bool ShowAll { get; set; }

        public VMarkingInputDto()
        {
            SubjectId = -1;
            PublishType = -1;
            MarkingStatus = -1;
            ShowAll = false;
        }
    }

    public class VMarkingDto : DDto
    {
        public string Batch { get; set; }
        public PublishType PublishType { get; set; }
        public string PaperId { get; set; }
        public string PaperCode { get; set; }
        public string PaperTitle { get; set; }
        public PaperType PaperType { get; set; }
        public int SubjectId { get; set; }
        public string Subject { get; set; }
        public DGroupDto Group { get; set; }
        public UserDto User { get; set; }
        public DateTime Time { get; set; }
        public byte Status { get; set; }
        public MarkingStatus MarkingStatus { get; set; }
        public int CountA { get; set; }
        public int CountB { get; set; }
    }
}
