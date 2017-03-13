using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 消息队列 消息类型 </summary>
    public enum RMessageType : byte
    {
        [Description("一般消息")]
        Nromal = 0,
        [Description("改错消息")]
        Correct = 1,
        [Description("全班通知")]
        Notice = 2,
        [Description("批阅通知")]
        MarkedNotice = 3,
        [Description("试卷排名通知")]
        PaperMarked = 4
    }
}
