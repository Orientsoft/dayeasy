using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary>
    /// 班级统计状态
    /// </summary>
    public enum ClassStatisticsStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("删除")]
        Delete = 4
    }

    /// <summary>
    /// 学生统计状态
    /// </summary>
    public enum StuStatisticsStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("删除")]
        Delete = 4
    }
}
