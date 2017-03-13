using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_Subject : DEntity
    {
        public TS_Subject()
        {
            TC_Video = new HashSet<TC_Video>();
            TQ_Question = new HashSet<TQ_Question>();
            TS_Knowledge = new HashSet<TS_Knowledge>();
        }

        [Key]
        [Column("SubjectID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public string SubjectName { get; set; }
        public string Icon { get; set; }
        public bool IsLoadFormula { get; set; }
        public string QTypeIDs { get; set; }
        public byte Status { get; set; }

        public virtual ICollection<TC_Video> TC_Video { get; set; }
        public virtual ICollection<TQ_Question> TQ_Question { get; set; }
        public virtual ICollection<TS_Knowledge> TS_Knowledge { get; set; }
    }
}
