using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 帖子排序 </summary>
    public enum TopicOrder : byte
    {
        [Description("时间升序")]
        TimeAsc = 0,
        [Description("时间降序")]
        TimeDesc = 1,
        [Description("浏览量")]
        ReadNum = 2,
        [Description("回复量")]
        ReplyNum = 3,
        [Description("点赞量")]
        PraiseNum = 4
    }
}
