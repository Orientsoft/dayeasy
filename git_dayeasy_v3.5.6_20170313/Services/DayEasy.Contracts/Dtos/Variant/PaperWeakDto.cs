using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Variant
{
    /// <summary> 试卷薄弱点 </summary>
    public class PaperWeakDto : DDto
    {
        /// <summary> 薄弱知识点 </summary>
        public Dictionary<string, int> Knowledges { get; set; }

        /// <summary> 错因标签 </summary>
        public Dictionary<string, int> ErrorTags { get; set; }

        public PaperWeakDto()
        {
            Knowledges = new Dictionary<string, int>();
            ErrorTags = new Dictionary<string, int>();
        }
    }
}
