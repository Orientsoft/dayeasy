using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_QuestionType : DEntity
    {
        public TS_QuestionType()
        {
            this.TQ_Question = new HashSet<TQ_Question>();
        }

        [Key]
        [Column("QTypeID")]
        [DatabaseGenerated((DatabaseGeneratedOption.Identity))]
        public override int Id { get; set; }
        public string QTypeName { get; set; }
        public string QTypeRemark { get; set; }
        public byte QTypeStyle { get; set; }
        public bool HasSmallQuestion { get; set; }
        public bool HasMultiAnswer { get; set; }
        public byte Status { get; set; }

        public virtual ICollection<TQ_Question> TQ_Question { get; set; }
    }
}
