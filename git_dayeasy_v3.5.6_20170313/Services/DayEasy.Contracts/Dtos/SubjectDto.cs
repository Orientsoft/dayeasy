
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    /// <summary> 科目 </summary>
    [AutoMapFrom(typeof(TS_Subject))]
    public class SubjectDto : DDto
    {
        public int Id { get; set; }
        /// <summary> 科目 </summary>
        [MapFrom("SubjectName")]
        public string Name { get; set; }
        /// <summary> 是否加载公式 </summary>
        public bool IsLoadFormula { get; set; }
        /// <summary> 题型列表 </summary>
        public int[] QuestionTypes { get; set; }
    }
}
