using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 帖子状态 </summary>
    public enum TopicStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("关闭")]
        Close = 1,
        [Description("删除")]
        Delete = 4
    }
}
