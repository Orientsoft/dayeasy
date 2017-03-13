using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DayEasy.Models.Open.Paper;

namespace DayEasy.Models.Open.Work
{
    /// <summary>
    /// 考试后-为试卷错题推送变式题
    /// </summary>
    public class MPaperVariantDto : DDto
    {
        /// <summary> 是否已推送变式 </summary>
        public bool IsSendVariant { get; set; }

        /// <summary> 薄弱知识点 </summary>
        public Dictionary<string, int> Knowledges { get; set; }

        /// <summary> 错因标签 </summary>
        public Dictionary<string, int> ErrorTags { get; set; }

        /// <summary> 题目字典 </summary>
        public Dictionary<string, MQuestionDto> Questions { get; set; }

        /// <summary> 题目变式 </summary>
        public List<MQuestionVariantDto> Variants { get; set; }

        public MPaperVariantDto()
        {
            Knowledges = new Dictionary<string, int>();
            ErrorTags = new Dictionary<string, int>();
            Variants = new List<MQuestionVariantDto>();
            Questions = new Dictionary<string, MQuestionDto>();
        }
    }

    /// <summary> 题目变式 </summary>
    public class MQuestionVariantDto : DDto
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

        public MQuestionVariantDto()
        {
            VariantIds = new List<string>();
        }
    }
}
