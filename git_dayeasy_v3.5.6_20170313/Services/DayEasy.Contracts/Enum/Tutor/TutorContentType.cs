
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 辅导内容类型 </summary>
    public enum TutorContentType : byte
    {
        [Description("文本")]
        Text = 0,
        [Description("视频")]
        Video = 1,
        [Description("题目")]
        Question = 2,
        [Description("特性与解法")]
        Feature = 3
    }
}
