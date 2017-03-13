using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 错题对应变式题 </summary>
    public class TP_VariantQuestion : DEntity<string>
    {
        [Key]
        [Column("VariantID")]
        public override string Id { get; set; }
        public string ErrorQID { get; set; }
        public string QuestionID { get; set; }
        public System.DateTime AddedAt { get; set; }

        public virtual TP_ErrorQuestion TP_ErrorQuestion { get; set; }
    }
}
