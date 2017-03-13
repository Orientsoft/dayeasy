using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.ThirdPlatform;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Account.Controllers
{
    /// <summary> 帐号绑定 </summary>
    [RoutePrefix("bind")]
    public class AccountBindController : DController
    {
        public AccountBindController(IUserContract userContract)
            : base(userContract)
        {
        }

        #region Views

        /// <summary> 帐号绑定页面 </summary>
        [Route("")]
        public ActionResult Index(string code, int t, string host = null, string back = null)
        {
            if (string.IsNullOrEmpty(code) || t < 0 || t > 4)
                return Redirect(Consts.Config.LoginSite);
            if (!string.IsNullOrWhiteSpace(host))
            {
                return Redirect(string.Format("{0}?t={1}&code={2}&back={3}", host, t, code, (back ?? string.Empty)));
            }
            //获取第三方登录用户信息
            var type = (PlatformType)t;
            var plat = PlatformFactory.GetInstance(type);
            var userResult = plat.User(code);
            if (!userResult.Status)
                return Redirect(Consts.Config.LoginSite);
            var data = userResult.Data;
            var dto = new PlatformDto
            {
                PlatformId = data.Id,
                AccessToken = data.AccessToken,
                Nick = data.Nick,
                PlatformType = t,
                Profile = data.Profile,
                UserId = UserId
            };
            var loginResult = UserContract.Login(dto, Comefrom.Web);
            if (!loginResult.Status)
                return MessageView(loginResult.Message);
            var loginData = loginResult.Data;
            if (loginData.Token != null)
            {
                //登录成功
                return Redirect(string.IsNullOrWhiteSpace(back) ? Consts.Config.MainSite : back);
            }

            //待绑定或新建
            if (CurrentUser != null)
            {
                var restult = UserContract.AccountBind(loginData.PlatId, CurrentUser.Id);
                if (restult.Status)
                    return Redirect(Consts.Config.AccountSite);
                return MessageView(restult.Message, returnUrl: Consts.Config.AccountSite, returnText: "个人设置");
            }
            dto.PlatformId = loginData.PlatId;
            return View(dto);
        }

        [Route("child")]
        [RoleAuthorize(UserRole.Parents, "/", false)]
        public ActionResult BindChild(string code, int t = -1)
        {
            if (!CurrentUser.IsParents() || (Children != null && Children.Any()))
                return Redirect(Consts.Config.MainSite);
            if (!string.IsNullOrWhiteSpace(code) && t >= 0)
            {
                //获取第三方登录用户信息
                var type = (PlatformType)t;
                var plat = PlatformFactory.GetInstance(type);
                var userResult = plat.User(code);
                if (userResult.Status)
                {
                    var result = UserContract.LoadChildByPlatform(new PlatformDto
                    {
                        PlatformId = userResult.Data.Id,
                        PlatformType = t,
                        AccessToken = userResult.Data.AccessToken
                    });
                    if (result.Status)
                    {
                        ViewBag.StudentId = result.Data.Id;
                    }
                }
            }
            return View();
        }

        [Route("child-complete")]
        public ActionResult BindChildComplete(long studentId)
        {
            var user = UserContract.Load(studentId);
            return PartialView(user);
        }

        #endregion

        #region Ajax

        /// <summary> 绑定帐号 </summary>
        [HttpPost]
        [Route("account-bind")]
        public ActionResult AccountBind(string platId, string account, string pwd)
        {
            var result = UserContract.AccountBind(platId, account, pwd);
            return DeyiJson(result, true);
        }

        /// <summary> 新用户 </summary>
        [HttpPost]
        [Route("account-create")]
        public ActionResult AccountCreate(string platId)
        {
            var result = UserContract.CreatePlatAccount(platId);
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("load-student")]
        public ActionResult LoadStudent(string account, string pwd)
        {
            var result = UserContract.LoadChild(account, pwd);
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("concat-student")]
        public ActionResult ConcatStudent(long studentId, int type)
        {
            var result = UserContract.BindChild(UserId, studentId, (FamilyRelationType)type);
            return DeyiJson(result, true);
        }

        #endregion
    }
}