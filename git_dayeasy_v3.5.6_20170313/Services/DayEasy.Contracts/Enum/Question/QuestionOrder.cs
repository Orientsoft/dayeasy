
namespace DayEasy.Contracts.Enum
{
    /// <summary> 试题排序 </summary>
    public enum QuestionOrder : byte
    {
        AddedAtDesc = 1,

        /// <summary> 出题时间顺序 </summary>
        AddedAt = 2,

        /// <summary> 分享时间倒序 </summary>
        ShareTimeDesc = 3,

        /// <summary> 分享时间顺序 </summary>
        ShareTime = 4,

        /// <summary> 错误次数倒序 </summary>
        ErrorCountDesc = 5,

        /// <summary> 错误次数顺序 </summary>
        ErrorCount = 6,

        /// <summary> 答题次数倒序 </summary>
        AnswerCountDesc = 7,

        /// <summary> 答题次数顺序 </summary>
        AnswerCount = 8,

        /// <summary> 难度系数 </summary>
        Difficulty = 9,

        /// <summary> 难度系数倒序 </summary>
        DifficultyDesc = 10,

        /// <summary> 错误率 </summary>
        ErrorRate = 11,

        /// <summary> 错误率倒序 </summary>
        ErrorRateDesc = 12,

        /// <summary> 使用次数 </summary>
        UsedCount = 13,

        /// <summary> 使用次数倒序 </summary>
        UsedCountDesc = 14,

        /// <summary> 随机排序 </summary>
        Random = 101
    }
}
