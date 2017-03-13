
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 用户机构状态 </summary>
    public enum UserAgencyStatus : byte
    {
        /// <summary> 当前在读/在职 </summary>
        [Description("当前在读/在职")]
        Current = 0,
        /// <summary> 就读/任教历史 </summary>
        [Description("就读/任教历史")]
        History = 1,
        /// <summary> 目标学校 </summary>
        [Description("目标学校")]
        Target = 2
    }
}
