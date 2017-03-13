﻿using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 分配任务初始化模型 </summary>
    public class JointAllotDto : DDto
    {
        /// <summary> 题目列表 </summary>
        public List<DistributeSectionDto> Sections { get; set; }

        /// <summary> 教师列表 </summary>
        public Dictionary<long, DUserDto> Teachers { get; set; }

        /// <summary> 任务列表 </summary>
        public List<AllotMissionDto> Missions { get; set; }
    }

    /// <summary> 分配题目信息 </summary>
    public class DistributeSectionDto : DDto
    {
        /// <summary> A/B卷 </summary>
        public string Section { get; set; }

        /// <summary> 题型，题目序号 </summary>
        public Dictionary<string, Dictionary<string, int>> Questions { get; set; }
    }

    /// <summary> 批阅任务 </summary>
    public class AllotMissionDto : DDto
    {
        /// <summary> 分组ID </summary>
        public string Id { get; set; }

        /// <summary> 试卷类型 </summary>
        public byte SectionType { get; set; }

        /// <summary> 题目列表 </summary>
        public Dictionary<string, int> Questions { get; set; }

        /// <summary> 教师列表 </summary>
        public List<long> Teachers { get; set; }
    }
}
