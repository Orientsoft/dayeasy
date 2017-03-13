

using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.Api.Attributes
{
    /// <summary> 基础身份验证 </summary>
    public class DApiAuthorizeAttribute : DApiAttribute
    {
        private UserDto _user;
        private readonly UserRole _role;
        private bool _baseResult;

        public DApiAuthorizeAttribute(UserRole role = UserRole.Caird)
        {
            //将学生权限扩展给家长
            if ((role & UserRole.Student) > 0)
                role |= UserRole.Parents;
            _role = role;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            _baseResult = base.IsAuthorized(actionContext);
            if (!_baseResult)
            {
                return false;
            }
            var controller = actionContext.ControllerContext.Controller as DApiController;
            if (controller != null)
            {
                _user = controller.CurrentUser;
            }
            if (controller == null || controller.CurrentUser == null)
            {
                return false;
            }
            if (_role != UserRole.Caird && (_user.Role & (int)_role) == 0)
                return false;
            return true;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (!_baseResult)
            {
                base.HandleUnauthorizedRequest(actionContext);
                return;
            }
            if (_user == null)
            {
                var msg = "接口需要登录";
                if (!string.IsNullOrWhiteSpace("token".QueryOrForm(string.Empty)))
                    msg = "登录已失效，请重新登录";
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK,
                    DResult.Error(msg));
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK,
                    DResult.Error("用户没有权限访问！"));
            }
        }
    }
}
