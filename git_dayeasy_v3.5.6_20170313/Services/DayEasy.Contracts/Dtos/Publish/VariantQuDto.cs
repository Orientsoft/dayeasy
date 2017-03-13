using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Publish
{
    public class VariantQuDto : DDto
    {
        public VariantQuDto()
        {
            Questions = new List<QuestionDto>();
            VariantQuDic = new Dictionary<string, List<string>>();
            DeyiVariantQuDic = new Dictionary<string, List<string>>();
        }
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public byte SourceType { get; set; }
        public string PaperName { get; set; }
        public string TeacherName { get; set; }
        public List<QuestionDto> Questions { get; set; }
        public Dictionary<string, List<string>> VariantQuDic { get; set; }
        public Dictionary<string, List<string>> DeyiVariantQuDic { get; set; }
    }
}
