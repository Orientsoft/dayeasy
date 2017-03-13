using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Publish
{
    /// <summary>
    /// 推荐辅导
    /// </summary>
    public class RecommendTutorDto : DDto
    {
        public RecommendTutorDto()
        {
            Tutors = new List<TutorItemDto>();
        }
        public string PaperId { get; set; }
        public string PaperName {get; set; }
        public string Batch { get; set; }
        public byte SourceType { get; set; }
        public List<TutorItemDto> Tutors { get; set; } 
    }

    public class TutorItemDto : DDto
    {
        public string TutorId { get; set; }
        public string TutorName { get; set; }
        public string FrontCover { get; set; }
        public int UsedCount { get; set; }
    }
}
