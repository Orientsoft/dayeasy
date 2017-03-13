
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 异常类型 </summary>
    public enum JointExceptionType : byte
    {
        /// <summary> 图片不是对应题目 </summary>
        [Description("图片不是对应题目")] NotCorrespond = 1,

        /// <summary> 试卷严重偏移/倾斜 </summary>
        [Description("试卷严重偏移/倾斜")] Offset = 2,

        /// <summary> AB卷混乱 </summary>
        [Description("AB卷混乱")] Mull = 3,

        /// <summary> 试卷重叠、题目内容被遮盖 </summary>
        [Description("试卷重叠、题目内容被遮盖")] Cover = 4,

        /// <summary> 其他 </summary>
        [Description("其他")] Other = 0
    }
}
