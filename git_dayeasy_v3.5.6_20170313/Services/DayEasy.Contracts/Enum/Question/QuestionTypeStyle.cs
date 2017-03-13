using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 试题结构类型 </summary>
    [Flags]
    public enum QuestionTypeStyle : byte
    {
        /// <summary> 题干 </summary>
        [Description("题干")]
        Body = 0,

        /// <summary> 小问 </summary>
        [Description("小问")]
        Detail = 1,

        /// <summary> 选项 </summary>
        [Description("选项")]
        Option = 2,

        /// <summary> 答案 </summary>
        [Description("答案")]
        Answer = 4
    }
}
