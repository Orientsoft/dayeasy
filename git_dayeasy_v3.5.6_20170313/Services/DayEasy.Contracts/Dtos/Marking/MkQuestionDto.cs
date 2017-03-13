using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Marking
{
    public class MkQuestionBaseDto : DDto
    {
        public string Id { get; set; }
        public decimal Score { get; set; }
        public bool Objective { get; set; }
        public int Sort { get; set; }
    }

    public class MkSmallQuestionDto : MkQuestionBaseDto{}

    public class MkQuestionDto : MkQuestionBaseDto
    {
        public string SectionId { get; set; }
        public List<MkSmallQuestionDto> SmallQuestions { get; set; }
    }
}
