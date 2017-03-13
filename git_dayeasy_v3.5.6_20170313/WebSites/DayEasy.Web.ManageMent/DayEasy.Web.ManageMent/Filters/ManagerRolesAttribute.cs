using System;
using System.Web;
using System.Web.Mvc;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Core;

namespace DayEasy.Web.ManageMent.Filters
{
    /// <summary> 后台管理角色过滤器 </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ManagerRolesAttribute : AdminAuthorizeAttribute
    {
        private readonly ManagerRole _managerRole;
        private long _currentManagerRole;

        public ManagerRolesAttribute(ManagerRole role = ManagerRole.None)
        {
            _managerRole = role;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var controller = filterContext.Controller as AdminController;
            if (controller == null) return;
            _currentManagerRole = controller.Role;
            base.OnAuthorization(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var result = base.AuthorizeCore(httpContext);
            if (!result) return false;
            if ((_currentManagerRole & (long)_managerRole) == 0)
                return false;
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            if ((_currentManagerRole & (long)_managerRole) == 0)
            {
                filterContext.Result = new RedirectResult(Consts.Config.AdminSite);
            }
        }
    }
}