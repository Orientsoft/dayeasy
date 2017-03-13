using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary> 协同进度 </summary>
    public class JointScheduleDto : DDto
    {
        /// <summary> 是否发起人 </summary>
        public bool IsOwner { get; set; }
        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }
        /// <summary> A卷/普通卷是否可批阅 </summary>
        public bool MarkingA { get; set; }
        /// <summary> B卷是否可批阅 </summary>
        public bool MarkingB { get; set; }
        /// <summary> 我的批阅进度 </summary>
        public List<UserScheduleDto> MyScheduleList { get; set; }

        /// <summary> 批阅进度列表 </summary>
        public List<JointScheduleItemDto> ScheduleList { get; set; }
    }

    /// <summary> 批阅进度 </summary>
    public class JointScheduleItemDto : DDto
    {
        /// <summary> AB卷类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 全客观题 </summary>
        public bool AllObjective { get; set; }

        /// <summary> 已批阅数量 </summary>
        public int Count { get; set; }

        /// <summary> 总数量 </summary>
        public int Total { get; set; }

        /// <summary> 分组题目序号 </summary>
        public List<string> Questions { get; set; }

        /// <summary> 进度详细 id:数量，name:教师名称 </summary>
        public List<TeacherMission> Missions { get; set; }
    }

    /// <summary> 用户进度 </summary>
    public class UserScheduleDto : DDto
    {
        /// <summary> AB卷类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 已批阅数量 </summary>
        public int Count { get; set; }

        /// <summary> 待批阅数量 </summary>
        public int WaitCount { get; set; }

        /// <summary> 分组题目序号 </summary>
        public List<string> Questions { get; set; }

        /// <summary> 阅卷包ID </summary>
        public string BagId { get; set; }
    }

    /// <summary> 教师任务 </summary>
    public class TeacherMission : DUserDto
    {
        /// <summary> 批阅数量 </summary>
        public int Count { get; set; }
    }
}
