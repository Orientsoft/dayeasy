using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Question
{
    [AutoMapFrom(typeof(TS_QuestionType))]
    public class QuestionTypeDto : DDto
    {
        public int Id { get; set; }
        [MapFrom("QTypeName")]
        public string Name { get; set; }
        [MapFrom("QTypeStyle")]
        public int Style { get; set; }
        [MapFrom("HasMultiAnswer")]
        public bool Multi { get; set; }
    }
}
