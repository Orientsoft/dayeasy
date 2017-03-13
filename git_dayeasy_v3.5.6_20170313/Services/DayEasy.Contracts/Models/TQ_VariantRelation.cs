using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary>
    /// ��ʽ����Ĺ�ϵ
    /// </summary>
    public class TQ_VariantRelation : DEntity<string>
    {
        public string QID { get; set; }
        public string VID { get; set; }
        public int UseCount { get; set; }
    }
}
