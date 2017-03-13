using DayEasy.Core.Domain;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary> 搜索圈子Dto </summary>
    public class SearchGroupDto : DPage
    {
        /// <summary> 圈子类型 </summary>
        public List<int> Types { get; set; }

        /// <summary> 关键字，自动匹配圈号规则 </summary>
        public string Keyword { get; set; }
        /// <summary> 机构ID </summary>
        public string AgencyId { get; set; }
        /// <summary> 认证等级 </summary>
        public byte?[] CertificationLevels { get; set; }
        /// <summary> 入学年份(仅班级圈) </summary>
        public int GradeYear { get; set; }

        /// <summary> 科目ID </summary>
        public int SubjectId { get; set; }

        public SearchGroupDto()
        {
            Types = new List<int>();
            CertificationLevels = null;
        }
    }
}
