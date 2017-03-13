using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 认证类型 </summary>
    [Flags]
    public enum ValidationType : byte
    {
        [Description("未验证")] None = 0,
        [Description("邮箱验证")] Email = 1,
        [Description("手机验证")] Mobile = 2,
        [Description("第三方登录")] Third = 4
    }
}
