using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Models.Open.Work
{
    public class MErrorAnalysisInputDto
    {
        public string ErrorId { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }

        public List<string> TagList
        {
            get { return string.IsNullOrWhiteSpace(Tags) ? new List<string>() : Tags.Split(',').Distinct().ToList(); }
        }
    }
}
