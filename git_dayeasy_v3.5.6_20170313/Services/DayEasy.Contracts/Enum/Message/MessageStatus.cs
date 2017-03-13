using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 消息状态 </summary>
    public enum MessageStatus : byte
    {
        /// <summary> 正常 </summary>
        [Description("正常")]
        Normal = 0,

        /// <summary> 已阅 </summary>
        [Description("已阅")]
        Read = 1,

        /// <summary> 已删除 </summary>
        [Description("已删除")]
        Delete = 4
    }
}
