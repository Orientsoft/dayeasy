using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary>
    /// 答卷Result
    /// </summary>
    public class MkResultDto : DDto
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public long StudentId { get; set; }
        public string ClassId { get; set; }
        public decimal TotalScore { get; set; }
        public bool IsFinished { get; set; }
        public List<MkDetailDto> Details { get; set; }
        public byte SectionType { get; set; }
        [IgnoreDataMember]
        public string SheetAnswers { get; set; }
        [IgnoreDataMember]
        public List<TP_ErrorQuestion> ErrorQuestions { get; set; }

        public MkResultDto()
        {
            Details = new List<MkDetailDto>();
            ErrorQuestions = new List<TP_ErrorQuestion>();
        }
    }
}
