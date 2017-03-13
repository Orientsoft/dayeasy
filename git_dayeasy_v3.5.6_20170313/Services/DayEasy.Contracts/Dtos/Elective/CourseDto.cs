using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Elective
{
    /// <summary> 选修课实体 </summary>
    public class CourseDto : DDto
    {
        /// <summary> 选修批次 </summary>
        public string Batch { get; set; }
        /// <summary> 标题 </summary>
        public string Title { get; set; }

        /// <summary> 是否能选课 </summary>
        public bool Selectable { get; set; }
        /// <summary> 课程列表 </summary>
        public List<CourseItemDto> Courses { get; set; }
    }

    /// <summary> 选修课程实体 </summary>
    public class CourseItemDto : DDto
    {
        /// <summary> 课程ID </summary>
        public int Id { get; set; }

        /// <summary> 课程名称 </summary>
        public string Name { get; set; }

        /// <summary> 上课教师 </summary>
        public string Teacher { get; set; }

        /// <summary> 上课地点 </summary>
        public string Address { get; set; }

        /// <summary> 总人数 </summary>
        public int Capacity { get; set; }

        /// <summary> 已选人数 </summary>
        public int SelectedCount { get; set; }

        /// <summary> 班级列表 </summary>
        public string ClassList { get; set; }
        /// <summary> 是否已选 </summary>
        public bool Selected { get; set; }
    }
}
