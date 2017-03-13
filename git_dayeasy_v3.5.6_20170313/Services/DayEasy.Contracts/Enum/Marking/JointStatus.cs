using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 协同阅卷状态 </summary>
    [Flags]
    public enum JointStatus : byte
    {
        /// <summary> 已发布 </summary>
        [Description("已发布")]
        Normal = 0,

        /// <summary> 已完成 </summary>
        [Description("已完成")]
        Finished = 2,

        /// <summary> 已删除 </summary>
        [Description("已删除")]
        Delete = 4
    }
}
