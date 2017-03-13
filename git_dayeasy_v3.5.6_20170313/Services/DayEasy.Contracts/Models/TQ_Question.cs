using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TQ_Question : DEntity<string>
    {
        public TQ_Question()
        {
            TP_PaperContent = new HashSet<TP_PaperContent>();
            TQ_AgencyQuestion = new HashSet<TQ_AgencyQuestion>();
            TQ_SmallQuestion = new HashSet<TQ_SmallQuestion>();
        }
        [Key]
        [Column("QID")]
        public override string Id { get; set; }

        [ForeignKey("TS_QuestionType")]
        public int QType { get; set; }
        public string QContent { get; set; }
        public string QImages { get; set; }
        public string QSummary { get; set; }

        [ForeignKey("TS_Subject")]
        public int SubjectID { get; set; }
        public string KnowledgeIDs { get; set; }
        public string TagIDs { get; set; }
        public byte Stage { get; set; }
        public bool IsObjective { get; set; }
        public byte ShareRange { get; set; }
        public byte Status { get; set; }
        public int UsedCount { get; set; }
        public long AnswerCount { get; set; }
        public long ErrorCount { get; set; }

        [ForeignKey("TU_User")]
        public long AddedBy { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIP { get; set; }
        public string ChangeSourceID { get; set; }
        public bool HasSmallQuestion { get; set; }
        public decimal DifficultyStar { get; set; }
        public bool? IsUsed { get; set; }
        public byte? OptionStyle { get; set; }
        public DateTime? LastModifyTime { get; set; }

        public virtual ICollection<TP_PaperContent> TP_PaperContent { get; set; }
        public virtual ICollection<TQ_AgencyQuestion> TQ_AgencyQuestion { get; set; }
        public virtual TS_QuestionType TS_QuestionType { get; set; }
        public virtual TS_Subject TS_Subject { get; set; }
        public virtual TU_User TU_User { get; set; }
        public virtual ICollection<TQ_SmallQuestion> TQ_SmallQuestion { get; set; }
    }
}
