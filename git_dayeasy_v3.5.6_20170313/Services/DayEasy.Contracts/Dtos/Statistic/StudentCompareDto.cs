using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;
using Newtonsoft.Json;

namespace DayEasy.Contracts.Dtos.Statistic
{
    /// <summary> 学生成绩对比 </summary>
    public class StudentCompareDto : DDto
    {
        /// <summary> 错题对比 </summary>
        public List<ErrorCompareDto> Errors { get; set; }

        /// <summary> 知识点对比 </summary>
        public List<KnowledgeCompareDto> Knowledges { get; set; }

        /// <summary> 构造函数 </summary>
        public StudentCompareDto()
        {
            Errors = new List<ErrorCompareDto>();
            Knowledges = new List<KnowledgeCompareDto>();
        }
    }

    /// <summary> 错题对比 </summary>
    public class ErrorCompareDto : DDto
    {
        /// <summary> 问题ID </summary>
        public string Id { get; set; }
        /// <summary> 用于排序 </summary>
        [JsonIgnore]
        public int Index { get; set; }
        /// <summary> 题号 </summary>
        public string Sort { get; set; }
        /// <summary> 我的正误：0,错;1,半对;2,对; </summary>
        public byte Mine { get; set; }
        /// <summary> 对方的正误：0,错;1,半对;2,对; </summary>
        public byte Other { get; set; }
    }

    /// <summary> 知识点得分率对比 </summary>
    public class KnowledgeCompareDto : DDto
    {
        /// <summary> 知识点ID </summary>
        public string Code { get; set; }
        /// <summary> 知识点 </summary>
        public string Knowledge { get; set; }

        /// <summary> 我的得分率 </summary>
        public int Mine { get; set; }

        /// <summary> 对方的得分率 </summary>
        public int Other { get; set; }
    }
}
