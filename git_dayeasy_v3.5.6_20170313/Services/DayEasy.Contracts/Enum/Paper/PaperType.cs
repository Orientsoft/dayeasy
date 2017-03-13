using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 试卷类型 </summary>
    public enum PaperType : byte
    {
        [Description("常规卷")]
        Normal = 1,
        [Description("AB卷")]
        AB = 2
    }
}
