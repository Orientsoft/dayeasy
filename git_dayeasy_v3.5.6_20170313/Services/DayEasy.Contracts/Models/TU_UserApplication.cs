using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_UserApplication : DEntity<string>
    {
        [ForeignKey("TU_User")]
        public long UserID { get; set; }
        [ForeignKey("TS_Application")]
        public int ApplicationID { get; set; }

        /// <summary> »ú¹¹ID </summary>
        [StringLength(32)]
        public string AgencyId { get; set; }
        public int Status { get; set; }
        public System.DateTime AddedAt { get; set; }
        [Required]
        public string AddedIP { get; set; }

        public virtual TS_Application TS_Application { get; set; }
        public virtual TU_User TU_User { get; set; }
    }
}
