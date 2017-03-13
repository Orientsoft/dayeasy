using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_MarkingMark : DEntity<string>
    {
        [Key]
        public override string Id { get; set; }

        /// <summary> ��������/Эͬ���� </summary>
        [StringLength(32)]
        public string BatchNo { get; set; }
        public byte PaperType { get; set; }
        public string Mark { get; set; }
    }
}
