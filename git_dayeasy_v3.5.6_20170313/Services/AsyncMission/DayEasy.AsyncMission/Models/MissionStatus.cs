using System.ComponentModel;

namespace DayEasy.AsyncMission.Models
{
    public enum MissionStatus : byte
    {
        /// <summary> 挂起 </summary>
        [Description("挂起")]
        Pendding = 0,
        /// <summary> 执行中 </summary>
        [Description("执行中")]
        Running = 1,
        /// <summary> 已完成 </summary>
        [Description("已完成")]
        Finished = 2,
        /// <summary> 已失效 </summary>
        [Description("已失效")]
        Invalid = 3
    }
}
