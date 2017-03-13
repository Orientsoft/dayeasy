using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 难度等级 </summary>
    public enum DiffLevel : byte
    {
        [Description("简单")]
        Easy = 0,
        [Description("中等")]
        Normal = 1,
        [Description("困难")]
        Diff = 2
    }
}
