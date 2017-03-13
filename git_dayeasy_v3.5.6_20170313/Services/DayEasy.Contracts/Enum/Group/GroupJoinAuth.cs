using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 加圈模式 </summary>
    public enum GroupJoinAuth : byte
    {
        /// <summary> 开放模式 </summary>
        [Description("开放模式")]
        Public = 0,
        /// <summary> 审核模式 </summary>
        [Description("审核模式")]
        Private = 1
    }
}
