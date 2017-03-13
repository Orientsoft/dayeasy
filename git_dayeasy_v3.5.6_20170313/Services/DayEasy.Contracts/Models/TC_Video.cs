using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TC_Video : DEntity<string>
    {
        public TC_Video()
        {
            TC_AgencyVideo = new HashSet<TC_AgencyVideo>();
        }

        [Key]
        [Column("VideoID")]
        public override string Id { get; set; }
        public string VideoName { get; set; }
        public string VideoDescription { get; set; }
        public string VideoUrl { get; set; }
        public string FrontCover { get; set; }

        [ForeignKey("TU_User")]
        public long UserId { get; set; }
        public string Speaker { get; set; }
        public Nullable<decimal> Duration { get; set; }
        public byte ShareRange { get; set; }

        [ForeignKey("TS_Subject")]
        public int SubjectId { get; set; }
        public string KnowPointIds { get; set; }
        public string TagIDs { get; set; }
        public byte Stage { get; set; }
        public byte VideoStatus { get; set; }
        public int UsedCount { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIp { get; set; }
        public Nullable<bool> IsUsed { get; set; }
        public byte Grade { get; set; }

        public virtual ICollection<TC_AgencyVideo> TC_AgencyVideo { get; set; }
        public virtual TS_Subject TS_Subject { get; set; }
        public virtual TU_User TU_User { get; set; }
    }
}
