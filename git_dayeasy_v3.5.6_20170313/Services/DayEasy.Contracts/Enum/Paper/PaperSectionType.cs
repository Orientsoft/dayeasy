using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 卷类型 </summary>
    public enum PaperSectionType : byte
    {
        [Description("A卷")]
        PaperA = 1,
        [Description("B卷")]
        PaperB = 2
    }
}
