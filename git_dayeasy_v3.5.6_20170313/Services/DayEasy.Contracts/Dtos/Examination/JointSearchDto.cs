using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 协同查询实体类 </summary>
    public class JointSearchDto : DPage
    {
        /// <summary> 机构ID </summary>
        public string AgencyId { get; set; }
        /// <summary> 科目ID </summary>
        public int Subject { get; set; }
        /// <summary> 协同状态 </summary>
        public JointStatus Status { get; set; }

        public JointSearchDto()
        {
            Subject = -1;
            Status = JointStatus.Finished;
        }
    }
}
