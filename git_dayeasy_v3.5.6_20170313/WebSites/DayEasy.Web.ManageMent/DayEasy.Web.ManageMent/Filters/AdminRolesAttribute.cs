using System;
using System.Web;
using System.Web.Mvc;
using DayEasy.Contracts.Enum;
using DayEasy.Core;

namespace DayEasy.Web.ManageMent.Filters
{
    /// <summary> 后台管理角色过滤器 </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminRolesAttribute : AdminAuthorizeAttribute
    {
        private readonly AdminRole _adminRole;
        private long _currentAdminRole;

        public AdminRolesAttribute(AdminRole role = AdminRole.Admin)
        {
            _adminRole = role;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var controller = filterContext.Controller as AdminController;
            if (controller == null) return;
            _currentAdminRole = controller.UserAdminRole;
            base.OnAuthorization(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var result = base.AuthorizeCore(httpContext);
            if (!result) return false;
            if ((_currentAdminRole & (long) _adminRole) == 0)
                return false;
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            if ((_currentAdminRole & (long)_adminRole) == 0)
            {
                filterContext.Result = new RedirectResult(Consts.Config.AdminSite);
            }
        }
    }
}