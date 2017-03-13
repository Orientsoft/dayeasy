using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 性别 </summary>
    public enum Gender : byte
    {
        [Description("保密")]
        Secret = 0,
        [Description("男")]
        Male = 1,
        [Description("女")]
        Female = 2

    }
}
