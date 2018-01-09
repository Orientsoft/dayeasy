using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DayEasy.Web
{
    /// <summary> Controller基类 </summary>
    public abstract class DController : Controller
    {
        protected readonly IUserContract UserContract;

        protected DController(IUserContract userContract)
        {
            UserContract = userContract;
        }

        protected HttpContext CurrentContext
        {
            get { return System.Web.HttpContext.Current; }
        }

        /// <summary> 原始Url </summary>
        protected string RawUrl
        {
            get
            {
                return Utils.RawUrl();
            }
        }

        /// <summary> 从Request的InputStream中加载实体 </summary>
        protected T FromBody<T>()
        {
            if (CurrentContext == null) return default(T);
            var input = CurrentContext.Request.InputStream;
            input.Seek(0, SeekOrigin.Begin);
            using (var stream = new StreamReader(input))
            {
                var body = stream.ReadToEnd();
                return JsonHelper.Json<T>(body, NamingType.CamelCase);
            }
        }

        /// <summary> 小驼峰式JsonResult </summary>
        protected ActionResult DeyiJson(object data, bool denyGet = false)
        {
            return DJson.Json(data, denyGet, namingType: NamingType.CamelCase);
        }

        protected ActionResult MessageView(string message, string title = null, string returnUrl = null,
            string returnText = null)
        {
            ViewBag.Title = title;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ReturnText = returnText;
            ViewBag.Message = message;
            return View("~/Views/Error/ErrorMsg.cshtml");
        }

        #region 当前登录用户信息

        private UserDto _currentUser;

        /// <summary>
        /// 当前登录用户基本信息
        /// </summary>
        public UserDto CurrentUser
        {
            get
            {
                if (_currentUser != null)
                    return _currentUser;
                var token = "";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = CookieHelper.GetValue(Consts.UserCookieName);
                }
                if (string.IsNullOrWhiteSpace(token) || token.Length != 32)
                    return null;
                return _currentUser = UserContract.Load(token);
            }
        }

        private List<UserRole> _userRoles;

        /// <summary> 当前登录用户角色集合 </summary>
        public List<UserRole> CurrentUserRoles
        {
            get
            {
                if (_userRoles != null)
                    return _userRoles;
                if (CurrentUser == null)
                    return new List<UserRole>();
                if (CurrentUser.Role == (byte)UserRole.Caird)
                    return new List<UserRole> { UserRole.Caird };
                return
                    _userRoles = Enum.GetValues(typeof(UserRole))
                        .Cast<UserRole>()
                        .Where(userRole => ((byte)userRole & CurrentUser.Role) > 0)
                        .ToList();
            }
        }

        /// <summary> 用户Id </summary>
        protected long UserId
        {
            get { return CurrentUser == null ? 0 : CurrentUser.Id; }
        }

        /// <summary> 用户或孩子Id </summary>
        protected long ChildOrUserId
        {
            get
            {
                if (Children != null && Children.Any())
                    return Children.First().Id;
                return UserId;
            }
        }

        /// <summary> 注册时间 </summary>
        protected DateTime ChildOrUserRegistTime
        {
            get
            {
                if (Children != null && Children.Any())
                {
                    var child = UserContract.Load(Children.First().Id);
                    if (child != null)
                    {
                        return child.AddedAt;
                    }
                }
                return CurrentUser.AddedAt;
            }
        }

        private List<DKeyValue<int, string>> _stages;

        /// <summary> 学段列表 </summary>
        public List<DKeyValue<int, string>> StageList
        {
            get
            {
                if (_stages != null)
                    return _stages;
                var stages = SystemCache.Instance.Stages(CurrentUser.SubjectId);
                return _stages = Enum.GetValues(typeof(StageEnum))
                    .Cast<StageEnum>()
                    .Where(t => stages.Contains((byte)t))
                    .Select(t => new DKeyValue<int, string>((int)t, t.GetText())).ToList();
            }
        }

        private List<ApplicationDto> _userApps;

        /// <summary> 用户应用 </summary>
        public List<ApplicationDto> UserApps
        {
            get
            {
                if (_userApps != null)
                    return _userApps;
                if (CurrentUser == null)
                    return (_userApps = new List<ApplicationDto>());
                if (CurrentUser.IsParents())
                {
                    //孩子的应用
                    if (Children != null && Children.Any())
                        return (_userApps = UserContract.UserApplications(Children.First().Id));
                    return (_userApps = new List<ApplicationDto>());
                }
                return (_userApps = UserContract.UserApplications(UserId));
            }
        }

        private List<RelationUserDto> _children;

        /// <summary> 孩子们 </summary>
        public List<RelationUserDto> Children
        {
            get
            {
                if (_currentUser == null || !_currentUser.IsParents())
                    return null;
                if (_children != null)
                    return _children;
                return _children = UserContract.Children(UserId);
            }
        }

        private UserAgencyDto _currentAgency;

        /// <summary> 当前机构 </summary>
        public UserAgencyDto CurrentAgency
        {
            get
            {
                if (_currentAgency != null)
                    return _currentAgency;
                if (_currentUser == null || ChildOrUserId <= 0)
                    return null;
                var result = UserContract.CurrentAgency(ChildOrUserId);
                return !result.Status ? null : (_currentAgency = result.Data);
            }
        }

        #endregion

        #region 重写Action执行前操作

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (CurrentUser != null)
            {
                ViewData["User"] = CurrentUser;
                if (CurrentUser.IsParents() && Children != null && Children.Any())
                    ViewData["Child"] = Children[0];
                ViewData["Roles"] = CurrentUserRoles;
                ViewData["Apps"] = UserApps;
            }
        }

        #endregion

    }
}
