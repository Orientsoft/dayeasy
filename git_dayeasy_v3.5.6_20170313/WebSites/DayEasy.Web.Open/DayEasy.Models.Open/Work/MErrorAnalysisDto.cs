
using System.Collections.Generic;

namespace DayEasy.Models.Open.Work
{
    public class MErrorAnalysisDto : DDto
    {
        public string ErrorId { get; set; }
        public string Content { get; set; }
        public List<string> TagList { get; set; }

        public MErrorAnalysisDto()
        {
            TagList = new List<string>();
        }
    }
}
