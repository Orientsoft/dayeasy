using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.LearningMemo
{
    public class LikeDetailDto : DDto
    {
        public string Name { get; set; }

        public bool IsGood { get; set; }
    }
}
