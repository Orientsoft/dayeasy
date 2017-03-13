using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_Class : DEntity<string>
    {
        [Key]
        [Column("ClassID")]
        public override string Id { get; set; }
        public string ClassName { get; set; }
        public string Logo { get; set; }
        public byte Stage { get; set; }
        public int GraduateYear { get; set; }
        public int Branch { get; set; }

        [ForeignKey("TU_Agency")]
        public string AgencyID { get; set; }
        public string Brief { get; set; }
        public int Capacity { get; set; }
        public Nullable<long> ClassManagerID { get; set; }
        public byte Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateIP { get; set; }
        public int Sort { get; set; }
        public string ClassCode { get; set; }

        public virtual TU_Agency TU_Agency { get; set; }
    }
}
