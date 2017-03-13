using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 内容类型 </summary>
    public enum ContentType : byte
    {
        [Description("试卷")] Paper = 0,
        [Description("课堂")] Class = 1,
        [Description("题目")] Question = 2,
        [Description("视频")] Video = 3,
        [Description("发布")] Publish = 4
    }
}
