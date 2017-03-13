using System;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Web.Filters;

namespace DayEasy.Web.ManageMent.Filters
{
    /// <summary> 后台管理过滤器 </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeAttribute : RoleAuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
            : base(UserRole.SystemManager, Consts.Config.MainSite, false)
        {
        }
    }
}
