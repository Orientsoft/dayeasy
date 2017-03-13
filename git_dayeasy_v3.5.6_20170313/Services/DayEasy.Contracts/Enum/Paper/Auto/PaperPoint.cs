
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 试卷知识点 </summary>
    public enum PaperPoint : byte
    {
        [Description("知识点随机取数据的知识点数量阀值")]
        RandomCount = 5,
        [Description("知识点随机取数据的低于比例阀值（0.0%）")]
        AveragePercent = 20
    }
}
