using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TQ_SmallQuestion : DEntity<string>
    {
        [Key]
        [Column("SmallQID")]
        public override string Id { get; set; }

        [ForeignKey("TQ_Question")]
        public string QID { get; set; }
        public string SmallQContent { get; set; }
        public bool IsObjective { get; set; }
        public int Sort { get; set; }
        public string SmallQImages { get; set; }
        public Nullable<byte> OptionStyle { get; set; }

        public virtual TQ_Question TQ_Question { get; set; }
    }
}
