using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{

    public class TU_User : DEntity<long>
    {
        public TU_User()
        {
            TC_ClassContentRecord = new HashSet<TC_ClassContentRecord>();
            TC_Usage = new HashSet<TC_Usage>();
            TC_Video = new HashSet<TC_Video>();
            TC_VideoClass = new HashSet<TC_VideoClass>();
            TP_ErrorQuestion = new HashSet<TP_ErrorQuestion>();
            TP_Paper = new HashSet<TP_Paper>();
            TQ_Question = new HashSet<TQ_Question>();
            TU_UserApplication = new HashSet<TU_UserApplication>();
            TU_UserAgencyRelation = new HashSet<TU_UserAgencyRelation>();
            TU_UserToken = new HashSet<TU_UserToken>();
        }
        [Key]
        [Column("UserID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }

        [StringLength(16)]
        public string UserCode { get; set; }
        public string NickName { get; set; }
        public string TrueName { get; set; }
        public string HeadPhoto { get; set; }
        public int? SubjectID { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime? Birthday { get; set; }
        public byte? Gender { get; set; }
        public int? Area { get; set; }
        public byte Role { get; set; }
        public byte Status { get; set; }
        public System.DateTime AddedAt { get; set; }
        public string AddedIp { get; set; }
        public Nullable<System.DateTime> LastLoginAt { get; set; }
        public string LastLoginIP { get; set; }
        public byte ValidationType { get; set; }
        public string StudentNum { get; set; }

        public byte CertificationLevel { get; set; }

        [StringLength(128)]
        public string Signature { get; set; }
        [StringLength(32)]
        public string AgencyId { get; set; }

        public int VisitCount { get; set; }
        public int GoodCount { get; set; }

        [StringLength(128)]
        public string Banner { get; set; }

        public virtual ICollection<TC_ClassContentRecord> TC_ClassContentRecord { get; set; }
        public virtual ICollection<TC_Usage> TC_Usage { get; set; }
        public virtual ICollection<TC_Video> TC_Video { get; set; }
        public virtual ICollection<TC_VideoClass> TC_VideoClass { get; set; }
        public virtual ICollection<TP_ErrorQuestion> TP_ErrorQuestion { get; set; }
        public virtual ICollection<TP_Paper> TP_Paper { get; set; }
        public virtual ICollection<TQ_Question> TQ_Question { get; set; }

        public virtual TS_StudentStatistic TS_StudentStatistic { get; set; }

        public virtual TS_TeacherStatistic TS_TeacherStatistic { get; set; }
        public virtual ICollection<TU_UserApplication> TU_UserApplication { get; set; }
        public virtual ICollection<TU_UserAgencyRelation> TU_UserAgencyRelation { get; set; }
        public virtual ICollection<TU_UserToken> TU_UserToken { get; set; }
    }
}
