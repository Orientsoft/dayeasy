using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DayEasy.Models.Open.Work
{
    public class MSendVariantDto : DDto
    {
        public string PaperId { get; set; }
        public string GroupIds { get; set; }
        public string Variants { get; set; }
        public long TeacherId { get; set; }
    }
}
