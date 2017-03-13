using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_AgencySubject : DEntity<string>
    {
        public string AgencyID { get; set; }
        public int GraduateYear { get; set; }
        public int Stage { get; set; }
        public string CurrentSubjects { get; set; }
        public string SubjectCollect { get; set; }
        public int Branch { get; set; }

        public virtual TU_Agency TU_Agency { get; set; }
    }
}
