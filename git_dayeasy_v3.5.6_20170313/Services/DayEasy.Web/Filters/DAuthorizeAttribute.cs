using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.Filters
{
    /// <summary> 平台基础身份过滤器 </summary>
    public class DAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary> 有权限的用户 </summary>
        public string AllowUser { get; set; }

        /// <summary> 授权失败时呈现的视图名称 </summary>
        public string View { get; set; }

        private readonly bool _allowCaird;
        private readonly bool _needAgency;
        private bool _hasChildren;

        protected UserDto User { get; set; }

        protected List<UserRole> UserRoles { get; set; }

        protected UserAgencyDto Agency { get; set; }

        protected bool HasRole(UserRole role)
        {
            return (User.Role & (byte)role) > 0;
        }

        /// <summary> 构造 </summary>
        /// <param name="allowCaird">允许游客</param>
        /// <param name="needAgency">需要有当前机构</param>
        public DAuthorizeAttribute(bool allowCaird = false, bool needAgency = true)
        {
            _allowCaird = allowCaird;
            _needAgency = needAgency;
        }

        #region 请求授权时执行

        /// <summary> 请求授权时执行 </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var actionDescriptor = filterContext.ActionDescriptor;
            var noAuthorizeAttributes = actionDescriptor.GetCustomAttributes(typeof(NoAuthorizeAttribute), true);

            if (noAuthorizeAttributes.Any())
                return;
            //登录用户身份验证
            var controller = filterContext.Controller as DController;
            if (controller == null) return;
            User = controller.CurrentUser;
            UserRoles = controller.CurrentUserRoles;
            Agency = controller.CurrentAgency;
            _hasChildren = (controller.Children != null && controller.Children.Any());
            base.OnAuthorization(filterContext);
        }

        #endregion

        #region 自定义授权检查

        /// <summary> 自定义授权检查 </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) 
                return false;
            //用户登录检查
            if (User == null) return false;
            if (!_allowCaird && User.Role == (byte)UserRole.Caird)
                return false;
            if (!_needAgency)
                return true;
            if (User.IsParents() && !_hasChildren)
                return false;
            return Agency != null;
        }

        #endregion

        #region 处理授权失败的处理

        /// <summary> 处理授权失败的处理 </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            var req = filterContext.HttpContext.Request;
            var rawUrl = req.RawUrl.TrimStart('/');
            var url = Consts.Config.LoginSite + "?return_url={0}";
            var returnUrl = string.Format("http://{0}/{1}", req.ServerVariables["HTTP_HOST"],
                rawUrl);
            if (User == null)
            {
                //异步请求
                if (req.IsAjaxRequest())
                {
                    if (req.UrlReferrer != null)
                        returnUrl = req.UrlReferrer.AbsoluteUri;
                    url = string.Format(url, filterContext.HttpContext.Server.UrlEncode(returnUrl));
                    var result = new JsonResult
                    {
                        Data = new { login = true, url },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    filterContext.Result = result;
                    return;
                }
                if (returnUrl.Contains(Consts.Config.MainSite) && string.IsNullOrWhiteSpace(rawUrl))
                    url = Consts.Config.LoginSite;
                else
                    url = string.Format(url,
                        filterContext.HttpContext.Server.UrlEncode(returnUrl));
                filterContext.Result = new RedirectResult(url);
                return;
            }

            if (!_allowCaird && User.Role == (byte)UserRole.Caird)
            {
                var redirectUrl = Consts.Config.MainSite + "/user/role?return_url=" + returnUrl.UrlEncode();
                if (req.IsAjaxRequest())
                {
                    AjaxRedirect(filterContext, redirectUrl);
                    return;
                }
                filterContext.Result = new RedirectResult(redirectUrl);
                return;
            }
            if (_needAgency && User.IsParents() && !_hasChildren)
            {
                var redirectUrl = Consts.Config.AccountSite + "/bind/child?return_url=" + returnUrl.UrlEncode();
                if (req.IsAjaxRequest())
                {
                    AjaxRedirect(filterContext, redirectUrl);
                    return;
                }
                filterContext.Result = new RedirectResult(redirectUrl);
                return;
            }
            if (_needAgency && Agency == null)
            {
                var redirectUrl = Consts.Config.MainSite + "/user/complete?return_url=" + returnUrl.UrlEncode();
                if (req.IsAjaxRequest())
                {
                    AjaxRedirect(filterContext, redirectUrl);
                    return;
                }
                filterContext.Result = new RedirectResult(redirectUrl);
            }
        }

        private void AjaxRedirect(AuthorizationContext filterContext, string url)
        {
            var result = new JsonResult
            {
                Data = new { redirect = true, url },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            filterContext.Result = result;
        }

        #endregion

    }
}