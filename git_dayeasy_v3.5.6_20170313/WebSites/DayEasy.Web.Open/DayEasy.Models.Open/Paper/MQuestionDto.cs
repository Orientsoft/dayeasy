
using System.Collections.Generic;

namespace DayEasy.Models.Open.Paper
{
    public class MQuestionDto : DDto
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public int Type { get; set; }
        /// <summary> 是否客观题 </summary>
        public bool IsObjective { get; set; }
        /// <summary> 是否是多选 </summary>
        public bool HasMulti { get; set; }
        public string[] Images { get; set; }
        public List<MDetailDto> Details { get; set; }
        public List<MAnswerDto> Answers { get; set; }
    }

    public class MDetailDto : DDto
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public int Sort { get; set; }
        public bool IsObjective { get; set; }
        public string[] Images { get; set; }
        public List<MAnswerDto> Answers { get; set; }
    }

    public class MAnswerDto : DDto
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public int Sort { get; set; }
        public string Tag { get; set; }
        public string[] Images { get; set; }
        public bool IsCorrect { get; set; }
    }
}
