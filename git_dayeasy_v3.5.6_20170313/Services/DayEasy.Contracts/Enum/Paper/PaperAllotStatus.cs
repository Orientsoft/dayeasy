using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 试卷分派记录状态 </summary>
    public enum PaperAllotStatus : byte
    {
        [Description("正常")]
        Finished = 0,
        [Description("已发送")]
        Sended = 1,
        [Description("已拒绝")]
        Refuse = 2
    }
}
