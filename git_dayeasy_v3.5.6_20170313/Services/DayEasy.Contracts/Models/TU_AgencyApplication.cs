using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_AgencyApplication : DEntity<string>
    {
        public string AgencyID { get; set; }
        public int ApplicationID { get; set; }
        public byte Status { get; set; }

        public virtual TS_Application TS_Application { get; set; }
        public virtual TU_Agency TU_Agency { get; set; }
    }
}
