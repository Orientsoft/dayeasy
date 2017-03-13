using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;

namespace DayEasy.AsyncMission.Models
{
    /// <summary> 异步任务 </summary>
    public class MAsyncMission : DEntity<string>
    {
        /// <summary> ID </summary>
        public override string Id { get; set; }

        /// <summary> 任务名称Type.FullName </summary>
        public string Name { get; set; }

        /// <summary> 任务类型 </summary>
        public MissionType Type { get; set; }
        /// <summary> 状态 </summary>
        public MissionStatus Status { get; set; }

        /// <summary> 优先级 </summary>
        public int Priority { get; set; }

        /// <summary> 参数 </summary>
        public string Params { get; set; }

        /// <summary> 失败次数 </summary>
        public int? FailCount { get; set; }

        /// <summary> 创建时间 </summary>
        public DateTime CreationTime { get; set; }

        /// <summary> 开始时间 </summary>
        public DateTime? StartTime { get; set; }

        public string Logs { get; set; }

        /// <summary> 完成时间 </summary>
        public DateTime? FinishedTime { get; set; }

        /// <summary> 创建者ID </summary>
        public long? CreatorId { get; set; }
        /// <summary> 任务消息 </summary>
        public string Message { get; set; }

        public MAsyncMission()
        {
            Status = MissionStatus.Pendding;
            CreationTime = Clock.Now;
        }

        public MAsyncMission(string name, MissionType type, MissionParam param, long? creator = null)
        {
            Id = IdHelper.Instance.Guid32;
            Name = name;
            Type = type;
            Params = JsonHelper.ToJson(param);
            CreatorId = creator;
            Status = MissionStatus.Pendding;
            CreationTime = Clock.Now;
        }
    }
}
