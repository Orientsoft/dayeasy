using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_Application : DEntity
    {
        public TS_Application()
        {
            TU_AgencyApplication = new HashSet<TU_AgencyApplication>();
            TU_UserApplication = new HashSet<TU_UserApplication>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("AppID")]
        public override int Id { get; set; }
        public string AppName { get; set; }
        public string AppURL { get; set; }
        public string AppIcon { get; set; }
        public string AppRemark { get; set; }
        public byte AppRoles { get; set; }
        public byte Status { get; set; }
        public Nullable<bool> IsSLD { get; set; }
        public byte AppType { get; set; }
        public int Sort { get; set; }

        public virtual ICollection<TU_AgencyApplication> TU_AgencyApplication { get; set; }
        public virtual ICollection<TU_UserApplication> TU_UserApplication { get; set; }
    }
}
