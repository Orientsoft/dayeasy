using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 发帖模式 </summary>
    public enum GroupPostAuth : byte
    {
        /// <summary> 仅圈主发帖 </summary>
        [Description("仅圈主发帖")]
        Owner = 0,
        /// <summary> 成员可发帖 </summary>
        [Description("成员可发帖")]
        Member = 9
    }
}
