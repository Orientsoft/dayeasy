using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DayEasy.Models.Open.Work
{
    public class MPubPaperDto : DDto
    {
        public string PaperId { get; set; }
        public string SourceGroupId { get; set; }
        public string GroupIds { get; set; }
        public string Message { get; set; }
    }
}
