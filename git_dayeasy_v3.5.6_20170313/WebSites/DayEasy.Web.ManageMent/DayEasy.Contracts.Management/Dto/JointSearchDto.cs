using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Management.Dto
{
    public class JointSearchDto : DPage
    {
        public string Keyword { get; set; }
        public string AgencyId { get; set; }
        public int SubjectId { get; set; }
        public int Status { get; set; }

        public bool IsAuth { get; set; }

        public JointSearchDto()
        {
            Status = -1;
            SubjectId = -1;
        }
    }
}
