using System;
using System.ComponentModel;

namespace DayEasy.Contracts.Enum
{
    /// <summary> 后台管理角色 </summary>
    [Flags]
    public enum AdminRole
    {
        [Description("无权限")] None = 0,

        /// <summary> 用户管理 </summary>
        [Description("用户管理")] UserManager = 0x1,

        /// <summary> 机构管理 </summary>
        [Description("机构管理")] AgencyManager = 0x2,

        /// <summary> 选修课管理 </summary>
        [Description("选修课管理")] ElectiveManager = 0x4,

        /// <summary> 系统数据管理 </summary>
        [Description("系统数据管理")] SystemManager = 0x8,

        /// <summary> 日志管理 </summary>
        [Description("日志管理")] LogManager = 0x10,

        /// <summary> 系统管理员权限分配 </summary>
        [Description("系统管理员权限分配")] Authority = 0x20,
        
        /// <summary> 首页配置管理 </summary>
        [Description("首页配置管理")] IndexManager = 0x40,

        /// <summary> 图文广告管理 </summary>
        [Description("图文广告管理")] AdvertManager = 0x80,

        /// <summary> 超级管理员 </summary>
        [Description("超级管理员")] Admin = 0xFFFFFFF
    }
}
