using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Elective
{
    /// <summary> 选修课 </summary>
    public class ElectiveInputDto : DDto
    {
        /// <summary> 标题 </summary>
        public string Title { get; set; }

        /// <summary> 机构ID </summary>
        public string AgencyId { get; set; }

        /// <summary> 用户ID </summary>
        public long UserId { get; set; }

        /// <summary> 课程列表 </summary>
        public List<CourseInputDto> Courses { get; set; }
    }
}
