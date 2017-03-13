using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 推荐精华类型 </summary>
    public enum TopicState : byte
    {
        [Description("常规贴")]
        Normal = 1,
        [Description("精华帖")]
        Pick = 2,
        [Description("推荐贴")]
        Recommend = 4
    }
}
