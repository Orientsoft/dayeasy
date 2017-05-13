using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 导入协同数据实体 </summary>
    public class JDataInputDto
    {
        /// <summary> 班级码 </summary>
        public string GroupCode { get; set; }
        /// <summary> 学生姓名 </summary>
        public string Student { get; set; }
        /// <summary> 分数 </summary>
        public List<decimal> Scores { get; set; }
    }
}
