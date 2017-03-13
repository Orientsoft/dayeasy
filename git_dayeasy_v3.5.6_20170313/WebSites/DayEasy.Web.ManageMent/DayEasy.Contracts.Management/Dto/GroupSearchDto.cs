
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Management.Dto
{
    public class GroupSearchDto : DPage
    {
        public string Keyword { get; set; }

        /// <summary> 圈子类型 </summary>
        public int Type { get; set; }

        /// <summary> 机构ID </summary>
        public string AgencyId { get; set; }

        /// <summary> 是否已认证 </summary>
        public int Level { get; set; }
        public int ClassType { get; set; }
        public bool ShowDelete { get; set; }

        public GroupSearchDto()
        {
            Type = -1;
            Level = -1;
            ClassType = -1;
        }
    }
}
