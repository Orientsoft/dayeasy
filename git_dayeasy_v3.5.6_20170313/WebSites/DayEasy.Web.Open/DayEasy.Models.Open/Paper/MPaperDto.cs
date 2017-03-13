
using System.Collections.Generic;

namespace DayEasy.Models.Open.Paper
{
    public class MPaperDto : DDto
    {
        public string Id { get; set; }
        public string PaperNo { get; set; }
        public string PaperTitle { get; set; }
        public byte PaperType { get; set; }
        public int SubjectId { get; set; }
        public List<MPaperSectionDto> Sections { get; set; }
    }

    public class MPaperSectionDto : DDto
    {
        public string SectionId { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public byte PaperSectionType { get; set; }
        public decimal SectionScore { get; set; }
        public List<MPaperQuestionDto> Questions { get; set; }
    }

    public class MPaperQuestionDto : MQuestionDto
    {
        public int Sort { get; set; }
        public decimal Score { get; set; }
    }
}
