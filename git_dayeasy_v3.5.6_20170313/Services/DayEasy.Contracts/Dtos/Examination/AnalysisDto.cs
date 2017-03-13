
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 分析基类 </summary>
    public class AnalysisDto : DDto
    {
        /// <summary> 班级ID </summary>
        public string ClassId { get; set; }
        /// <summary> 班级名称 </summary>
        public string ClassName { get; set; }
        /// <summary> 圈主 </summary>
        public string Teacher { get; set; }
        /// <summary> 总人数 </summary>
        public int TotalCount { get; set; }
        /// <summary> 参考人数 </summary>
        public int StudentCount { get; set; }
    }
}
