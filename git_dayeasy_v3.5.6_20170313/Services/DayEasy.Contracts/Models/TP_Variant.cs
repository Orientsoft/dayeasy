using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 教师推荐变式题 </summary>
    public class TP_Variant : DEntity<string>
    {
        /// <summary> 发布批次 </summary>
        public string Batch { get; set; }
        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }
        /// <summary> 问题ID </summary>
        public string QID { get; set; }
        /// <summary> 变式问题ID </summary>
        public string VIDs { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
    }
}
