
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 认证等级 </summary>
    public enum GroupCertificationLevel
    {
        /// <summary> 拒绝认证 </summary>
        [Description("已拒绝")]
        Refuse = 0,
        /// <summary> 已认证 </summary>
        [Description("已认证")]
        Normal = 1
    }
}
