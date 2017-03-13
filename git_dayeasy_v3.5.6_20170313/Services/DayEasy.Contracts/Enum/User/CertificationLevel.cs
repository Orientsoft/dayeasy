
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 认证等级 </summary>
    public enum CertificationLevel : byte
    {
        /// <summary> 普通 </summary>
        [Description("未认证")]
        Normal = 0,
        /// <summary> 官方认证 </summary>
        [Description("官方认证")]
        Official = 1
    }
}
