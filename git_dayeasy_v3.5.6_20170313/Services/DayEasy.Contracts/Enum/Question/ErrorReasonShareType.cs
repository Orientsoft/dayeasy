using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 错因分享范围 </summary>
    public enum ErrorReasonShareType : byte
    {
        [Description("公开")]
        Open = 0,
        [Description("仅自己和教师可见")]
        Limited = 2
    }
}
