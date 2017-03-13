using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 组合阅卷实体模型 </summary>
    public class JCombineDto : DDto
    {
        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }

        /// <summary> 试卷标题 </summary>
        public string PaperTitle { get; set; }
        /// <summary> 是否AB卷 </summary>
        public bool IsAb { get; set; }
        /// <summary> 题目信息 </summary>
        public Dictionary<string, JQuestionDto> Questions { get; set; }
        /// <summary> 分组信息 </summary>
        public List<JGroupDto> Groups { get; set; }

        /// <summary> 构造函数 </summary>
        public JCombineDto()
        {
            Questions = new Dictionary<string, JQuestionDto>();
            Groups = new List<JGroupDto>();
        }
    }

    /// <summary> 协同分组信息 </summary>
    public class JGroupDto : DDto
    {
        /// <summary> 试卷类型 </summary>
        public byte Section { get; set; }

        /// <summary> 题目ID列表 </summary>
        public List<string> QuestionIds { get; set; }

        /// <summary> 分组区域 </summary>
        public DArea Region { get; set; }
    }

    /// <summary> 协同题目信息 </summary>
    public class JQuestionDto : DDto
    {
        /// <summary> 题型 </summary>
        public int Type { get; set; }

        /// <summary> 序号 </summary>
        public string Sort { get; set; }

        /// <summary> 分数 </summary>
        public decimal Score { get; set; }

        /// <summary> 区域 </summary>
        public DArea Area { get; set; }
    }
}
