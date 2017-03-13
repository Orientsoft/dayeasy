using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_Knowledge : DEntity
    {
        [Key]
        [Column("KnowledgeID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public string Name { get; set; }
        public byte KnowledgeVersion { get; set; }
        public string FullPinYin { get; set; }
        public string SimplePinYin { get; set; }
        public string Code { get; set; }
        public int PID { get; set; }
        public int Sort { get; set; }
        public byte Stage { get; set; }

        [ForeignKey("TS_Subject")]
        public int SubjectID { get; set; }
        public byte Status { get; set; }
        public bool HasChildren { get; set; }

        public virtual TS_Subject TS_Subject { get; set; }
    }
}
