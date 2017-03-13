using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.ThirdPlatform;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Account.Helper;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Account.Controllers
{
    [DAuthorize(true)]
    public class HomeController : DController
    {
        public HomeController(IUserContract userContract)
            : base(userContract)
        { }

        #region Views

        [Route("")]
        public ActionResult Index()
        {
            var plats = UserContract.Platforms(UserId, (int)PlatformType.Tencent);
            if (plats != null && plats.Any())
            {
                ViewData["plat"] = plats.First();
            }
            var relations = new List<RelationUserDto>();
            if (CurrentUser.IsStudent())
            {
                relations = UserContract.Parents(UserId);
            }
            else if (CurrentUser.IsParents())
            {
                relations = Children;
            }
            ViewData["relations"] = relations;
            ViewBag.HasPwd = UserContract.HasPwd(UserId);
            return View();
        }

        [AjaxOnly]
        [Route("edit-email")]
        public ActionResult EditEmail()
        {
            return PartialView();
        }

        [AjaxOnly]
        [Route("bind-mobile")]
        public ActionResult BindMobile()
        {
            return PartialView();
        }

        [AjaxOnly]
        [Route("change-pwd")]
        public ActionResult ChangePwd()
        {
            ViewBag.HasPwd = UserContract.HasPwd(UserId);
            return PartialView(CurrentUser);
        }
        #endregion

        #region Ajax
        /// <summary> 保存邮箱 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("save-email")]
        public ActionResult SaveEmail(string code, string email)
        {
            var result = Helper.VCodeHelper.Check(email, code, string.Format(ValidateHelper.AccountCodeSession, email));
            if (!result.Status) return DeyiJson(result);
            if (!UserContract.CheckAccount(email).Status)
                return DeyiJson(DResult.Error("该邮箱已被使用"));
            return DeyiJson(UserContract.SaveEmail(CurrentUser.Id, email));
        }

        /// <summary> 保存手机 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("save-mobile")]
        public ActionResult SaveMobile(string code, string mobile)
        {
            var result = Helper.VCodeHelper.Check(mobile, code, string.Format(ValidateHelper.AccountCodeSession, mobile));
            if (!result.Status) return DeyiJson(result);
            if (!UserContract.CheckAccount(mobile).Status)
                return DeyiJson(DResult.Error("改手机已被使用"));
            return DeyiJson(UserContract.Update(new UserDto { Id = CurrentUser.Id, Mobile = mobile }));
        }

        /// <summary> 更新用户信息 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("update")]
        public ActionResult Update(string nick, string mobile, string avatar, string studentNo, string name)
        {
            var dto = new UserDto
            {
                Id = CurrentUser.Id
            };
            var update = false;
            if (!string.IsNullOrWhiteSpace(nick) && CurrentUser.Nick != nick)
            {
                dto.Nick = nick;
                update = true;
            }
            if (!string.IsNullOrWhiteSpace(mobile) && CurrentUser.Mobile != mobile)
            {
                dto.Mobile = mobile;
                update = true;
            }
            if (!string.IsNullOrWhiteSpace(avatar) && CurrentUser.Avatar != avatar)
            {
                dto.Avatar = avatar;
                update = true;
            }
            if (!string.IsNullOrWhiteSpace(studentNo) && CurrentUser.StudentNum != studentNo)
            {
                dto.StudentNum = studentNo;
                update = true;
            }
            if (!string.IsNullOrWhiteSpace(name) && CurrentUser.Name != name)
            {
                dto.Name = name;
                update = true;
            }
            var result = !update ? DResult.Error("没有任何修改~！") : UserContract.Update(dto);
            return DeyiJson(result, true);
        }

        /// <summary> 绑定第三方登录 跳转地址 </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Route("plat/bind/{type}")]
        public ActionResult BindThird(byte type)
        {
            var plat = PlatformFactory.GetInstance((PlatformType)type);
            return Redirect(plat.LoginUrl());
        }


        /// <summary>
        /// 解除第三方登录绑定
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("plat/unbind")]
        public ActionResult UnBindThird(byte type)
        {
            return DeyiJson(UserContract.CancelAccountBind(CurrentUser.Id, type), true);
        }

        [HttpPost]
        [Route("unbind-child")]
        public ActionResult UnBindChild(long id)
        {
            var result = UserContract.CancelBindRelation(UserId, id);
            return DeyiJson(result);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="oldPwd"></param>
        /// <param name="password"></param>
        /// <param name="confirmPwd"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("update-pwd")]
        public ActionResult UpdatePassword(string oldPwd, string password, string confirmPwd)
        {
            return DJson.Json(UserContract.ChangPwd(CurrentUser.Id, oldPwd, password, confirmPwd));
        }

        #region 发送验证码

        /// <summary> 发送邮件验证码 </summary>
        [HttpPost]
        [Route("send-email")]
        public ActionResult SendEmail()
        {
            var email = "email".Form(string.Empty);
            if (string.IsNullOrEmpty(email) || !email.As<IRegex>().IsEmail())
                return DJson.Json(DResult.Error("邮件不正确，请重新输入！"), true);
            if (!UserContract.CheckAccount(email).Status)
                return DJson.Json(DResult.Error("该邮箱已经使用了！"), true);

            var code = RandomHelper.Random().Next(100000, 999999).ToString(CultureInfo.InvariantCulture);
            var sendCode = Helper.VCodeHelper.Send(email, code, string.Format(ValidateHelper.AccountCodeSession, email));
            if (!sendCode.Status) return DeyiJson(sendCode);
            var emailServer = ValidateHelper.GetEmailServer(email);
            return DJson.Json(new DResult(true, "http://" + emailServer), true);
        }

        /// <summary> 发送短信验证码 </summary>
        [HttpPost]
        [Route("send-mobile")]
        public ActionResult SendMobile()
        {
            var mobile = "mobile".Form(string.Empty);
            if (string.IsNullOrEmpty(mobile) || !mobile.As<IRegex>().IsMobile())
                return DJson.Json(DResult.Error("手机号码格式不正确，请重新输入！"), true);

            if (!UserContract.CheckAccount(mobile).Status)
                return DeyiJson(DResult.Error("该手机已被使用"));

            var code = RandomHelper.Random().Next(100000, 999999).ToString(CultureInfo.InvariantCulture);
            var result = Helper.VCodeHelper.Send(mobile, code, string.Format(ValidateHelper.AccountCodeSession, mobile));
            return DeyiJson(result);
        }

        #endregion

        #endregion
    }
}