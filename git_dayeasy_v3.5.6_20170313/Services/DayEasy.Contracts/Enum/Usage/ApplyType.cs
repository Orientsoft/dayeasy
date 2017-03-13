using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 发布终端 </summary>
    [Flags]
    public enum ApplyType : byte
    {
        [Description("移动端")]
        Mobile = 1,
        [Description("Web端")]
        Web = 2,
        [Description("打印")]
        Print = 4
    }
}
