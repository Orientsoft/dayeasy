using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_SmallQScore : DEntity<string>
    {
        [Key]
        [Column("PaperID", Order = 1)]
        [ForeignKey("TP_Paper")]
        public override string Id { get; set; }
        [Key]
        [Column(Order = 2)]
        public string SmallQID { get; set; }
        public decimal Score { get; set; }

        public virtual TP_Paper TP_Paper { get; set; }
    }
}
