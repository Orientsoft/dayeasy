using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 辅导状态 </summary>
    public enum TutorStatus : byte
    {
        [Description("正常")]
        Nomal = 0,
        [Description("草稿")]
        Draft = 1,
        [Description("上架")]
        Grounding = 2,
        [Description("下架")]
        Undercarriage = 3,
        [Description("删除")]
        Delete = 4,
    }
}
