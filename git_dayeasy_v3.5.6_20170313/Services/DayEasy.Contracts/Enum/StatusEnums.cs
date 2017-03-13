
using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 普通状态(老数据，新数据不使用) </summary>
    [Obsolete]
    public enum TempStatus
    {
        [Description("正常")]
        Normal = 1,
        [Description("删除")]
        Delete = 4
    }

    /// <summary> 普通状态 </summary>
    public enum NormalStatus
    {
        [Description("正常")]
        Normal = 0,
        [Description("删除")]
        Delete = 4
        
    }

    /// <summary> 审核状态 </summary>
    public enum CheckStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("待审")]
        Pending = 1,
        [Description("拒绝")]
        Refuse = 2,
        [Description("无效")]
        Invalid = 3,
        [Description("删除")]
        Delete = 4
    }

    /// <summary> 用户状态 </summary>
    public enum UserStatus : byte
    {
        [Description("正常")]
        Normal = 0,
        [Description("未绑")]
        UnBind = 2,
        [Description("删除")]
        Delete = 4
    }

    /// <summary> 选修课状态 </summary>
    public enum ElectiveStatus : byte
    {
        /// <summary> 未开始 </summary>
        [Description("未开始")] Normal = 0,

        /// <summary> 进行中 </summary>
        [Description("进行中")] Started = 1,

        /// <summary> 已结束 </summary>
        [Description("已结束")] Finished = 2,

        /// <summary> 已删除 </summary>
        [Description("已删除")] Delete = 4
    }
}
