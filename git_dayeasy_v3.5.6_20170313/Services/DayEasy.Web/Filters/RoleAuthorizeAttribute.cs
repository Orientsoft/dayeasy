using System.Web;
using System.Web.Mvc;
using DayEasy.Contracts.Enum;

namespace DayEasy.Web.Filters
{
    /// <summary> 角色身份过滤器 </summary>
    public class RoleAuthorizeAttribute : DAuthorizeAttribute
    {
        private readonly string _redirect;
        private readonly UserRole _userRole;

        public RoleAuthorizeAttribute(UserRole role, string redirect, bool needAgency = true)
            : base(false, needAgency)
        {
            if ((role & UserRole.Student) > 0)//家长拥有学生的所有权限
            {
                role |= UserRole.Parents;
            }
            _userRole = role;
            _redirect = redirect;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var result = base.AuthorizeCore(httpContext);
            return result && HasRole(_userRole);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            if (User == null)
                return;
            filterContext.Result = new RedirectResult(_redirect);
        }
    }
}
