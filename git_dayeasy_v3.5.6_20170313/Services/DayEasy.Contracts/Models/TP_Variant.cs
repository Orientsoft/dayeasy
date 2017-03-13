using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> ��ʦ�Ƽ���ʽ�� </summary>
    public class TP_Variant : DEntity<string>
    {
        /// <summary> �������� </summary>
        public string Batch { get; set; }
        /// <summary> �Ծ�ID </summary>
        public string PaperId { get; set; }
        /// <summary> ����ID </summary>
        public string QID { get; set; }
        /// <summary> ��ʽ����ID </summary>
        public string VIDs { get; set; }
        public System.DateTime AddedAt { get; set; }
        public long AddedBy { get; set; }
    }
}
