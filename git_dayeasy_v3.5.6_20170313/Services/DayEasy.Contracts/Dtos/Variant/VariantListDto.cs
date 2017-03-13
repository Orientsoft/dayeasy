
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Variant
{
    /// <summary> 试卷变式列表 </summary>
    public class VariantListDto : DDto
    {
        /// <summary> 参考人数 </summary>
//        public int StudentCount { get; set; }

        /// <summary> 题目字典 </summary>
        public Dictionary<string, QuestionDto> Questions { get; set; }

        /// <summary> 题目变式 </summary>
        public List<QuestionVariantDto> Variants { get; set; }

        public VariantListDto()
        {
            Questions = new Dictionary<string, QuestionDto>();
            Variants = new List<QuestionVariantDto>();
        }
    }

    /// <summary> 题目变式 </summary>
    public class QuestionVariantDto : DDto
    {
        /// <summary> 题目Id </summary>
        public string Id { get; set; }

        /// <summary> 试卷类型 </summary>
        public byte SectionType { get; set; }
        /// <summary> 序号 </summary>
        public int Sort { get; set; }
        /// <summary> 错误次数 </summary>
        public int ErrorCount { get; set; }

        /// <summary> 变式题列表 </summary>
        public List<string> VariantIds { get; set; }

        public QuestionVariantDto()
        {
            VariantIds = new List<string>();
        }
    }
}
