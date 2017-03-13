
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 异常申报类型 </summary>
    public enum MarkingExceptionType : byte
    {
        /// <summary> 其他 </summary>
        [Description("其他")]
        Other = 0,

        /// <summary> 不一致 </summary>
        [Description("图片不是对应题目")]
        Atypism = 1,

        /// <summary> 偏移 </summary>
        [Description("试卷严重偏移/倾斜")]
        Deviation = 2,

        /// <summary> 混乱 </summary>
        [Description("AB卷混乱")]
        Chaos = 3,

        /// <summary> 重叠/遮盖 </summary>
        [Description("试卷重叠、题目内容被遮盖")]
        Overlap = 4,
    }
}
