
using System.ComponentModel;

namespace DayEasy.Contracts.Management.Enum
{
    public enum JointExceptionStatus : byte
    {
        [Description("未解决")]
        Normal = 0,
        [Description("已解决")]
        Solved = 4
    }
}
