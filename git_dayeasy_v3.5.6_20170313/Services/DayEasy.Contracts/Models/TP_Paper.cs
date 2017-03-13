using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_Paper : DEntity<string>
    {
        public TP_Paper()
        {
            TP_AgencyPaper = new HashSet<TP_AgencyPaper>();
            TP_MarkingResult = new HashSet<TP_MarkingResult>();
            TP_PaperContent = new HashSet<TP_PaperContent>();
            TP_PaperSection = new HashSet<TP_PaperSection>();
            TP_SmallQScore = new HashSet<TP_SmallQScore>();
        }

        [Key]
        [Column("PaperID")]
        public override string Id { get; set; }
        public string PaperTitle { get; set; }
        public byte PaperType { get; set; }
        public int SubjectID { get; set; }
        public string TagIDs { get; set; }
        public byte Stage { get; set; }
        public byte Grade { get; set; }
        public byte ShareRange { get; set; }
        public byte Source { get; set; }
        public byte Status { get; set; }
        public string ChangeSourceID { get; set; }

        [ForeignKey("TU_User")]
        public long AddedBy { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIP { get; set; }
        public string KnowledgeIDs { get; set; }
        public string PaperScores { get; set; }
        public bool IsUsed { get; set; }
        public int UseCount { get; set; }
        public string PaperNo { get; set; }

        public virtual ICollection<TP_AgencyPaper> TP_AgencyPaper { get; set; }
        public virtual ICollection<TP_MarkingResult> TP_MarkingResult { get; set; }
        public virtual ICollection<TP_PaperContent> TP_PaperContent { get; set; }
        public virtual ICollection<TP_PaperSection> TP_PaperSection { get; set; }
        public virtual TU_User TU_User { get; set; }
        public virtual ICollection<TP_SmallQScore> TP_SmallQScore { get; set; }
    }
}
