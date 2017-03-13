using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 批阅的试卷类型 </summary>
    public enum MarkingPaperType : byte
    {
        [Description("常规卷")]
        Normal = 0,
        [Description("A卷")]
        PaperA = 1,
        [Description("B卷")]
        PaperB = 2
    }
}
