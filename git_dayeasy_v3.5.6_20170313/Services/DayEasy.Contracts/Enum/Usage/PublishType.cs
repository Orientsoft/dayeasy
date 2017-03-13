using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 发布类型 </summary>
    public enum PublishType
    {
        [Description("视频")]
        Video = 0,
        [Description("检测")]
        Test = 1,
        [Description("打印试卷")]
        Print = 2
    }
}
