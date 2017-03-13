using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 错题状态 </summary>
    public enum ErrorQuestionStatus : byte
    {
        /// <summary> 正常 </summary>
        [Description("正常")]
        Normal = 0,
        /// <summary> 已标记 </summary>
        [Description("标记")]
        Marked = 1,
        /// <summary> 已过关 </summary>
        [Description("过关")]
        Pass = 2,
        /// <summary> 已删除 </summary>
        [Description("已删除")]
        Delete = 4
    }
}
