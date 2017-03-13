using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    public class CompleteMarkingInputDto : DDto
    {
        public string Batch { get; set; }
        public bool IsJoint { get; set; }
        public bool SetIcon { get; set; }
        public bool SetMarks { get; set; }
        public long UserId { get; set; }

        public CompleteMarkingInputDto()
        {
            SetIcon = true;
            SetMarks = true;
        }
    }
}
