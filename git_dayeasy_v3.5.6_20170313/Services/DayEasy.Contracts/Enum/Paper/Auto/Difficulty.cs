using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary>
    /// 难度为困难的题目难度系数占比(0.0%)
    /// </summary>
    public enum Difficulty : byte
    {
        [Description("题目难度系数为1,2 颗星占比(0.0%)")]
        Star12 = 10,
        [Description("题目难度系数为3 颗星占比(0.0%)")]
        Star3 = 60,
        [Description("题目难度系数为4,5 颗星占比(0.0%)")]
        Star45 = 30
    }
}
