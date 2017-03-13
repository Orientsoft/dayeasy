using System.ComponentModel;

namespace DayEasy.Utility.License
{
    /// <summary> 编码/激活码类型 </summary>
    public enum LicenseType
    {
        /// <summary> 登录码 </summary>
        [Description("登录码")]
        [CodeLength(9)]
        LoginCode = 0,

        /// <summary> 得一号 </summary>
        [Description("得一号")]
        [CodeLength(5)]
        DCode = 1,

        /// <summary> 圈号 </summary>
        [Description("圈号")]
        [CodeLength(5)]
        GroupCode = 2
    }
}
