using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Elective
{
    /// <summary> 选修课程 </summary>
    public class CourseInputDto : DDto
    {
        /// <summary> 课程名称 </summary>
        public string Name { get; set; }

        /// <summary> 上课教师 </summary>
        public string Teacher { get; set; }

        /// <summary> 上课地点 </summary>
        public string Address { get; set; }

        /// <summary> 班级容量 </summary>
        public int ClassCapacity { get; set; }

        /// <summary> 总人数 </summary>
        public int Capacity { get; set; }

        /// <summary> 班级列表 </summary>
        public List<string> ClassList { get; set; }
    }
}
