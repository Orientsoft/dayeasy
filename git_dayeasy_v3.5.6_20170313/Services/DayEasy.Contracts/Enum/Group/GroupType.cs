using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 圈子类型 </summary>
    public enum GroupType : byte
    {
        [Description("班级圈")]
        Class = 0,
        [Description("同事圈")]
        Colleague = 1,
        [Description("分享圈")]
        Share = 2
    }
}
