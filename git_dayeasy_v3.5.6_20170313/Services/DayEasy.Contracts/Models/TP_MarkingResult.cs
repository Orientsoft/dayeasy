using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_MarkingResult : DEntity<string>
    {
        public TP_MarkingResult()
        {
            TP_MarkingDetail = new HashSet<TP_MarkingDetail>();
        }

        [Key]
        [Column("MarkingID")]
        public override string Id { get; set; }

        [ForeignKey("TP_Paper")]
        [StringLength(32)]
        public string PaperID { get; set; }

        [ForeignKey("TC_Usage")]
        [StringLength(32)]
        public string Batch { get; set; }
        public long StudentID { get; set; }

        [StringLength(32)]
        public string ClassID { get; set; }
        public DateTime AddedAt { get; set; }
        public string AddedIP { get; set; }
        public bool IsFinished { get; set; }
        public long? MarkingBy { get; set; }
        public DateTime? MarkingTime { get; set; }
        public decimal TotalScore { get; set; }
        public string SectionScores { get; set; }
        public int ErrorQuestionCount { get; set; }

        public virtual TC_Usage TC_Usage { get; set; }
        public virtual ICollection<TP_MarkingDetail> TP_MarkingDetail { get; set; }
        public virtual TP_Paper TP_Paper { get; set; }
    }
}
