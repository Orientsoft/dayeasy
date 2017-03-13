using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary>
    /// 试卷发布dto
    /// </summary>
    public class PaperPublishDto : DDto
    {
        public string PaperId { get; set; }
        public string PaperName { get; set; }
        public string Time { get; set; }
    }
}
