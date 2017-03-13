
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 应用类型 </summary>
    public enum ApplicationType : byte
    {
        [Description("普通应用")]
        Normal = 0,
        [Description("附加应用")]
        Additional = 4,
        [Description("收费应用")]
        Paid = 8
    }
}
