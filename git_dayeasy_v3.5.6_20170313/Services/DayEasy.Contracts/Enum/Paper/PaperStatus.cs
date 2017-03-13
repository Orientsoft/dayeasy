using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 试卷状态 </summary>
    public enum PaperStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("草稿")]
        Draft = 1,
        [Description("删除")]
        Delete = 4
    }
}
