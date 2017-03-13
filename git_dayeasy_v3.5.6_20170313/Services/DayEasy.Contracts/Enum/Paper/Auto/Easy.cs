using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 难度为简单的题目难度系数占比(0.0%) </summary>
    public enum Easy : byte
    {
        [Description("题目难度系数为1,2 颗星占比(0.0%)")]
        Star12 = 60,
        [Description("题目难度系数为3 颗星占比(0.0%)")]
        Star3 = 30,
        [Description("题目难度系数为4,5 颗星占比(0.0%)")]
        Star45 = 10
    }
}
