using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Account.Helper;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using VCodeHelper = DayEasy.Utility.Helper.VCodeHelper;

namespace DayEasy.Web.Account.Controllers
{
    /// <summary> 注册 </summary>
    [RoutePrefix("reg")]
    public class RegController : DController
    {
        public RegController(IUserContract userContract) : base(userContract)
        {
        }

        #region Views

        [Route("")]
        public ActionResult Reg()
        {
            if (CurrentUser != null) return Redirect(Consts.Config.MainSite);
            ViewData["subjects"] = SystemCache.Instance.Subjects();
            return View();
        }

        public ActionResult Protocol()
        {
            return View();
        }

        /// <summary> 获取验证码 </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetIdentifyingCode()
        {
            return RedirectToAction("Vcode", "Login");
        }
        #endregion

        #region Ajax

        [HttpPost]
        public ActionResult SendCode(string account, string vcode)
        {
            if (string.IsNullOrEmpty(account) || (!account.As<IRegex>().IsEmail() && !account.As<IRegex>().IsMobile()))
                return DeyiJson(DResult.Error("帐号格式不正确"));
            if (string.IsNullOrEmpty(vcode) || !VCodeHelper.Verify(vcode, false))
                return DeyiJson(DResult.Error("验证码不正确"));
            if (!UserContract.CheckAccount(account).Status)
                return DeyiJson(DResult.Error("帐号已被使用"));
            var rcode = RandomHelper.Random().Next(100000, 999999).ToString(CultureInfo.InvariantCulture);
            var sessionKey = string.Format(ValidateHelper.AccountCodeSession, account);
            var result = Helper.VCodeHelper.Send(account, rcode, sessionKey);
            return DeyiJson(result);
        }

        public ActionResult SaveReg(string account, string password, string rcode, int role, int subject)
        {
            var isEmail = account.IsNotNullOrEmpty() && account.As<IRegex>().IsEmail();
            var isMobile = !isEmail && account.IsNotNullOrEmpty() && account.As<IRegex>().IsMobile();

            if (!isEmail && !isMobile)
                return DeyiJson(DResult.Error("帐号格式不正确"));
            if (string.IsNullOrEmpty(password) || !new Regex("^[a-zA-Z0-9_\\.\\@]{6,20}$").IsMatch(password))
                return DeyiJson(DResult.Error("密码格式不正确"));
            if (!UserContract.CheckAccount(account).Status)
                return DeyiJson(DResult.Error("帐号已被使用"));

            //免验证注册
            var key = "key".QueryOrForm("");
            var skip = key.IsNotNullOrEmpty() && key == ConfigHelper.GetConfigString("defSign");
            if (!skip)
            {
                var sessionKey = string.Format(ValidateHelper.AccountCodeSession, account);
                var ckCode = Helper.VCodeHelper.Check(account, rcode, sessionKey);
                if (!ckCode.Status) return DeyiJson(ckCode);
            }

            var registDto = new RegistUserDto
            {
                Email = isEmail ? account : string.Empty,
                Mobile = isMobile ? account : string.Empty,
                Password = password,
                Subject = subject,
                Role = (UserRole)role,
                ValidationType = isEmail ? ValidationType.Email : ValidationType.Mobile
            };
            if (skip) registDto.ValidationType = ValidationType.None;

            var result = UserContract.Regist(registDto);
            if (!result.Status)
                return DeyiJson(result);
            //注册成功，自动登录
            UserContract.AutoLogin(result.Data);
            return DeyiJson(DResult.Success);
        }

        #endregion

    }
}