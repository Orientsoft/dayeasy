using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Management.Enum
{
    /// <summary> 后台管理角色 </summary>
    [Flags]
    public enum ManagerRole
    {
        /// <summary> 无权限 </summary>
        [Description("无权限")]
        None = 0,

        /// <summary> 用户管理 </summary>
        [Description("用户管理")]
        Manager = 0x1,

        /// <summary> 会员管理 </summary>
        [Description("会员管理")]
        MemberManager = 0x2,

        /// <summary> 运营管理 </summary>
        [Description("运营管理")]
        OperationManager = 0x4,

        /// <summary> 系统数据管理 </summary>
        [Description("系统管理")]
        SystemManager = 0x8,

        /// <summary> 站点管理 </summary>
        [Description("站点管理")]
        SiteManager = 0x10,

        /// <summary> 日志管理 </summary>
        [Description("日志管理")]
        LogManager = 0x20,

        /// <summary> 超级管理员 </summary>
        [Description("超级管理员")]
        Admin = 0xFFFFFFF
    }
}
