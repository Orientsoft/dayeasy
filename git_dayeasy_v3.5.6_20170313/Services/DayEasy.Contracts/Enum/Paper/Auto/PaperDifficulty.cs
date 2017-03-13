using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 试卷难度 </summary>
    public enum PaperDifficulty : byte
    {
        [Description("简单")]
        Easy = 0,
        [Description("一般")]
        Normal = 1,
        [Description("困难")]
        Difficulty = 2
    }
}
