using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    public enum LogLevel : byte
    {
        [Description("调试")] Debug = 0,
        [Description("信息")] Info = 1,
        [Description("错误")] Error = 2,
        [Description("致命")] Fatal = 3
    }

    public enum LogType : byte
    {
        [Description("系统")] System = 0,
        [Description("题库")] Question = 1,
        [Description("试卷")] Paper = 2,
        [Description("课堂")] Video = 3,
    }
}
