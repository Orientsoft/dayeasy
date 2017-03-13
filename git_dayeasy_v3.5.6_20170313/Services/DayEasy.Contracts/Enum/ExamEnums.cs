
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 考试状态 </summary>
    public enum ExamStatus : byte
    {
        /// <summary> 普通 </summary>
        [Description("未推送")]
        Normal = 0,
        /// <summary> 已推送 </summary>
        [Description("已推送")]
        Sended = 1,
        /// <summary> 推送老师 </summary>
        [Description("推送教师")]
        SendToTeacher = 2,
        /// <summary> 已发布 </summary>
        [Description("发布成绩")]
        Published = 8,
        /// <summary> 已删除 </summary>
        [Description("已删除")]
        Delete = 4
    }

    /// <summary> 考试类型 </summary>
    public enum ExamType : byte
    {
        /// <summary> 普通协同 </summary>
        [Description("普通协同")]
        Joint = 0,
        /// <summary> 联考协同 </summary>
        [Description("联考协同")]
        Union = 1
    }
}
