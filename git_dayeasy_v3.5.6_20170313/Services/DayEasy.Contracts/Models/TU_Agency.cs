using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_Agency : DEntity<string>
    {
        public TU_Agency()
        {
            TC_AgencyVideo = new HashSet<TC_AgencyVideo>();
            TP_AgencyPaper = new HashSet<TP_AgencyPaper>();
            TQ_AgencyQuestion = new HashSet<TQ_AgencyQuestion>();
            TU_AgencySubject = new HashSet<TU_AgencySubject>();
            TU_AgencyApplication = new HashSet<TU_AgencyApplication>();
            TU_Class = new HashSet<TU_Class>();
        }

        [Key]
        [Column("AgencyID")]
        public override string Id { get; set; }
        public string AgencyName { get; set; }
        public string AgencyCode { get; set; }
        public string AgencyPaperFront { get; set; }
        public string AgencyPaperBack { get; set; }
        public byte AgencyType { get; set; }
        public string BadgeFile { get; set; }
        public Nullable<int> Area { get; set; }
        public string AgencyLogo { get; set; }
        public string HomePage { get; set; }
        public string Brief { get; set; }
        public byte Status { get; set; }
        public long ApplyBy { get; set; }
        public System.DateTime ApplyAt { get; set; }
        public string ApplyIP { get; set; }
        public Nullable<long> CheckBy { get; set; }
        public Nullable<System.DateTime> CheckPassTime { get; set; }
        public long ManagerID { get; set; }
        public string Address { get; set; }
        public string RefuseMsg { get; set; }
        public string Stages { get; set; }

        public virtual ICollection<TC_AgencyVideo> TC_AgencyVideo { get; set; }
        public virtual ICollection<TP_AgencyPaper> TP_AgencyPaper { get; set; }
        public virtual ICollection<TQ_AgencyQuestion> TQ_AgencyQuestion { get; set; }
        public virtual ICollection<TU_AgencySubject> TU_AgencySubject { get; set; }
        public virtual ICollection<TU_AgencyApplication> TU_AgencyApplication { get; set; }
        public virtual ICollection<TU_Class> TU_Class { get; set; }
    }
}
