using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{

    public class TC_VideoClass : DEntity<string>
    {
        public TC_VideoClass()
        {
            TC_VideoClassContent = new HashSet<TC_VideoClassContent>();
        }
        [Key]
        [Column("ClassRID")]
        public override string Id { get; set; }
        public string ClassRName { get; set; }
        public string ClassRDescription { get; set; }
        public string FrontCover { get; set; }
        public decimal TotalDuration { get; set; }
        public int MicrotestCount { get; set; }
        public int VideoCount { get; set; }
        [ForeignKey("TU_User")]
        public long UserId { get; set; }
        public int SubjectId { get; set; }
        public byte Stage { get; set; }
        public byte ClassRStatus { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIp { get; set; }
        public string Tags { get; set; }
        public byte Grade { get; set; }

        public virtual ICollection<TC_VideoClassContent> TC_VideoClassContent { get; set; }
        public virtual TU_User TU_User { get; set; }
    }
}
