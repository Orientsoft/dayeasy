using System;
using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Elective
{
    /// <summary> 选课详情 </summary>
    public class ElectiveDetailDto : DDto
    {
        /// <summary> 课程ID </summary>
        public int CourseId { get; set; }

        /// <summary> 课程名称 </summary>
        public string CourseName { get; set; }

        /// <summary> 容量 </summary>
        public int Capacity { get; set; }
        public string Teacher { get; set; }
        public string Address { get; set; }
        public int Current { get; set; }

        /// <summary> 学生列表 </summary>
        public List<ElectiveUserDto> Students { get; set; }
    }

    /// <summary> 选修学生信息 </summary>
    public class ElectiveUserDto : DDto
    {
        /// <summary> 学生ID </summary>
        public long Id { get; set; }

        /// <summary> 学生名字 </summary>
        public string Name { get; set; }

        /// <summary> 学生班级 </summary>
        public string ClassNmae { get; set; }

        /// <summary> 选课时间 </summary>
        public DateTime Time { get; set; }
    }
}
