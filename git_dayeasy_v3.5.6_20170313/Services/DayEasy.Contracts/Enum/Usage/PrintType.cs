using System;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 打印类型 </summary>
    [Flags]
    public enum PrintType
    {
        /// <summary> 作业 </summary>
        HomeWork = 1,

        /// <summary> 答题卡 </summary>
        AnswerSheet = 2,

        /// <summary> A卷作业 </summary>
        PaperAHomeWork = 4,

        /// <summary> A卷答题卡 </summary>
        PaperAAnswerSheet = 8,

        /// <summary> B卷作业 </summary>
        PaperBHomeWork = 16,

        /// <summary> B卷答题卡 </summary>
        PaperBAnswerSheet = 32,

        /// <summary> AB卷作业 </summary>
        PaperAbHomeWork = 64,

        /// <summary> AB卷答题卡 </summary>
        PaperAbAnswerSheet = 128,
    }
}
