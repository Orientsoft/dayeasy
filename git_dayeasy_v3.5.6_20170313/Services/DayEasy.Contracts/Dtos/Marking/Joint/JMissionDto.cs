
using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking.Joint
{
    /// <summary> 协同批阅任务 </summary>
    public class JMissionDto : DDto
    {
        /// <summary> 试卷ID </summary>
        public string PaperId { get; set; }
        /// <summary> 试卷标题 </summary>
        public string PaperTitle { get; set; }

        /// <summary> 是否AB卷 </summary>
        public bool IsAb { get; set; }

        /// <summary> 是否创建者 </summary>
        public bool IsCreator { get; set; }
        /// <summary> 我的任务 </summary>
        public List<MissionItemDto> Missions { get; set; }

        /// <summary> 圈内教师 </summary>
        public Dictionary<long, DUserDto> Teachers { get; set; }

        /// <summary> 问题对应编号 </summary>
        public Dictionary<string, string> QuestionSorts { get; set; }

        /// <summary> 年级批阅进度 </summary>
        public Dictionary<byte, Dictionary<string, List<QuestionMissionDto>>> QuestionMissions { get; set; }
        /// <summary> 报告异常数 </summary>
        public int ExceptionCount { get; set; }
        /// <summary> 已解决数 </summary>
        public int SolveCount { get; set; }

        /// <summary> 构造函数 </summary>
        public JMissionDto()
        {
            Missions = new List<MissionItemDto>();
            Teachers = new Dictionary<long, DUserDto>();
            QuestionSorts = new Dictionary<string, string>();
            QuestionMissions = new Dictionary<byte, Dictionary<string, List<QuestionMissionDto>>>();
        }
    }

    /// <summary> 单个任务 </summary>
    public class MissionItemDto : DDto
    {
        /// <summary> 试卷类型：0，普通，1，A卷，2，B卷 </summary>
        public byte SectionType { get; set; }

        /// <summary> 题目类型 </summary>
        public string QuestionType { get; set; }

        /// <summary> 是否有异常 </summary>
        public bool HasException { get; set; }

        /// <summary> 是否可阅 </summary>
        public bool Markingabel { get; set; }

        /// <summary> 问题列表 </summary>
        public List<string> QuestionIds { get; set; }

        /// <summary> 已阅数量 </summary>
        public int Marked { get; set; }

        /// <summary> 未阅数量 </summary>
        public int Left { get; set; }
        /// <summary> 百分比 </summary>
        public decimal Percent
        {
            get
            {
                if (Marked + Left <= 0)
                    return 0;
                return Math.Round((Marked / (decimal)(Marked + Left)), 2, MidpointRounding.AwayFromZero) * 100;
            }
        }
        /// <summary> 构造函数 </summary>
        public MissionItemDto()
        {
            QuestionIds = new List<string>();
        }
    }

    /// <summary> 单个问题进度 </summary>
    public class QuestionMissionDto : DDto
    {
        /// <summary> 问题Id </summary>
        public string Id { get; set; }

        /// <summary> 批阅百分比 </summary>
        public decimal Percent
        {
            get
            {
                if (Count <= 0)
                    return 0;
                return Math.Round(Marked / (decimal)Count, 2, MidpointRounding.AwayFromZero) * 100;
            }
        }

        /// <summary> 每人批阅数量 </summary>
        public Dictionary<long, int> Teachers { get; set; }
        /// <summary> 总份数 </summary>
        public int Count { get; set; }

        /// <summary> 已批阅数量 </summary>
        public int Marked { get { return Teachers.Values.Sum(); } }

        /// <summary> 未批阅数量 </summary>
        public int Left { get { return Count - Marked; } }

        /// <summary> 构造函数 </summary>
        public QuestionMissionDto()
        {
            Teachers = new Dictionary<long, int>();
        }
    }
}
