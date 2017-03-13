using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TP_ErrorQuestion : DEntity<string>
    {
        public TP_ErrorQuestion()
        {
            TP_VariantQuestion = new HashSet<TP_VariantQuestion>();
        }

        [Key]
        [Column("ErrorQID")]
        public override string Id { get; set; }
        public string PaperID { get; set; }
        public string PaperTitle { get; set; }
        public string Batch { get; set; }
        public string QuestionID { get; set; }
        [ForeignKey("TU_User")]
        public long StudentID { get; set; }
        public int SubjectID { get; set; }
        public byte Stage { get; set; }
        public int QType { get; set; }
        public string RemarkContent { get; set; }
        public string RemarkImages { get; set; }
        public int Importance { get; set; }
        public System.DateTime AddedAt { get; set; }
        public byte Status { get; set; }
        public int VariantCount { get; set; }
        public byte SourceType { get; set; }

        public virtual TU_User TU_User { get; set; }
        public virtual ICollection<TP_VariantQuestion> TP_VariantQuestion { get; set; }
    }
}
