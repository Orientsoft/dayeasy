using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 分享圈排序 </summary>
    public enum ShareGroupOrder : byte
    {
        /// <summary> 时间升序 </summary>
        [Description("时间升序")]
        AddAtAsc = 0,
        /// <summary> 时间倒序 </summary>
        [Description("时间倒序")]
        AddAtDesc = 1,
        /// <summary> 帖子倒序 </summary>
        [Description("帖子倒序")]
        TopicNumDesc = 2
    }
}
