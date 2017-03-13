using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;

namespace DayEasy.Application.Services.Dto
{
    /// <summary> 联考 - 图表模型 </summary>
    public class VUnionChartsDto : DDto
    {
        /// <summary> 大型考试ID </summary>
        public string Id { get; set; }

        /// <summary> 联考关联批次 </summary>
        public string Batch { get; set; }

        /// <summary> 标题 </summary>
        public string Title { get; set; }

        /// <summary> 涵盖科目 </summary>
        public Dictionary<int, string> Subjects { get; set; }
    }
}
