using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 发布试卷状态 </summary>
    [Flags]
    public enum UsagePaperStatus : byte
    {
        /// <summary> 进行中 </summary>
        [Description("进行中")]
        Working = 1,

        /// <summary> 已结束 </summary>
        [Description("已结束")]
        OverTime = 2,

        /// <summary> 批阅完成 </summary>
        [Description("批阅完成")]
        Marked = 4,

        /// <summary> 无人提交 </summary>
        [Description("无人提交")]
        NoSubmit = 8,

        /// <summary> 未开始 </summary>
        [Description("未开始")]
        UnStart = 16
    }
}
