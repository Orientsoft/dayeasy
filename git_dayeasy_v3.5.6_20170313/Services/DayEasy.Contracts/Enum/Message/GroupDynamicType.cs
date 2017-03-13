using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 圈内动态类型 </summary>
    public enum GroupDynamicType : byte
    {
        /// <summary> 作业 </summary>
        [Description("作业")]
        Homework = 0,

        /// <summary> 考试 </summary>
        [Description("考试")]
        Exam = 1,

        /// <summary> 通知 </summary>
        [Description("通知")]
        Notice = 2,

        /// <summary> 辅导 </summary>
        [Description("辅导")]
        Tutor = 3,

        /// <summary> 表扬 </summary>
        [Description("表扬")]
        Praise = 4,

        /// <summary> 协同阅卷 </summary>
        [Description("协同阅卷")]
        Joint = 5,
        /// <summary> 成绩通知 </summary>
        [Description("成绩通知")]
        ExamNotice = 6
    }
}
