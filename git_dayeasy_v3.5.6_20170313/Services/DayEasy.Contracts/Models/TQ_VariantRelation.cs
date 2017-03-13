using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary>
    /// 变式题核心关系
    /// </summary>
    public class TQ_VariantRelation : DEntity<string>
    {
        public string QID { get; set; }
        public string VID { get; set; }
        public int UseCount { get; set; }
    }
}
