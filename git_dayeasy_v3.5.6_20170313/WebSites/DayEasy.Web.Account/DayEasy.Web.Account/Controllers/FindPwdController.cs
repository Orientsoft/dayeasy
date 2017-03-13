using System;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Core;
using DayEasy.Core.Config;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Account.Helper;
using VCodeHelper = DayEasy.Utility.Helper.VCodeHelper;

namespace DayEasy.Web.Account.Controllers
{
    /// <summary> 密码找回 </summary>
    [RoutePrefix("find")]
    public class FindPwdController : DController
    {
        public FindPwdController(IUserContract userContract)
            : base(userContract)
        {
        }

        #region Views
        [Route("")]
        public ActionResult Index()
        {
            if (CurrentUser != null) return Redirect(Consts.Config.MainSite);
            return View();
        }

        /// <summary>
        /// 找回密码--step2--重置密码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("change")]
        public ActionResult ChangePwd()
        {
            if (CurrentUser != null) return Redirect(Consts.Config.MainSite);
            var account = SessionHelper.Get<string>(ValidateHelper.CheckPassUserIdSession);//经过第一步
            var accountFrom2 = SessionHelper.Get<string>(ValidateHelper.AccountSession);//第二步的缓存
            if (string.IsNullOrEmpty(account) && string.IsNullOrEmpty(accountFrom2))
                return RedirectToAction("Index", "FindPwd");

            if (!string.IsNullOrEmpty(account))
                SessionHelper.Remove(ValidateHelper.CheckPassUserIdSession);//清除第一步的缓存

            if (string.IsNullOrEmpty(accountFrom2))
                SessionHelper.Add(ValidateHelper.AccountSession, account, 60 * 2);//再次缓存帐号到第二步  2个小时

            ViewData["account"] = string.IsNullOrEmpty(account) ? accountFrom2 : account;

            return View();
        }

        /// <summary>
        /// 找回密码--step3--找回成功
        /// </summary>
        /// <returns></returns>
        [Route("success")]
        public ActionResult FindSuccess()
        {
            if (CurrentUser != null) return Redirect(Consts.Config.MainSite);
            var userId = SessionHelper.Get<long>(ValidateHelper.CheckedDyCodeSession);
            if (userId <= 0)//没有经过第二步
            {
                return Redirect(Consts.Config.LoginSite);
            }

            SessionHelper.Remove(ValidateHelper.CheckedDyCodeSession);//清除经过第二步的标记

            return View();
        }
        #endregion

        #region 修改密码--保存数据
        /// <summary>
        /// 修改密码--保存数据
        /// </summary>
        /// <param name="formCollection"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("complete")]
        public ActionResult FindPwdComplete(FormCollection formCollection)
        {
            var checkAccountObj = SessionHelper.Get<string>(ValidateHelper.AccountSession);
            if (string.IsNullOrWhiteSpace(checkAccountObj))
                return Json(DResult.Error("修改账户验证失败！"), JsonRequestBehavior.AllowGet);

            var account = "account".Form(string.Empty);
            if (checkAccountObj.Trim() != account.Trim())
                return Json(DResult.Error("修改账户验证失败！"), JsonRequestBehavior.AllowGet);

            string pwd = "pwd".Form(string.Empty); ;
            string confirmPwd = "confirmPwd".Form(string.Empty); ;

            var result = UserContract.ChangPwd(account, pwd, confirmPwd);
            if (result.Status)
            {
                SessionHelper.Remove(ValidateHelper.AccountSession);//清除第二步的帐号缓存

                SessionHelper.Add(ValidateHelper.CheckedDyCodeSession, result.Data, 24 * 60);//表示经过了第三步
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 验证账户

        /// <summary>
        /// 验证账户
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckEmail()
        {
            var account = "account".Form(string.Empty);
            if (!account.As<IRegex>().IsEmail() && !account.As<IRegex>().IsMobile())
                return Json(DResult.Error("请输入正确的邮箱或者手机！"), JsonRequestBehavior.AllowGet);

            var result = UserContract.ExistsAccount(account);
            return Json(result ? DResult.Success : DResult.Error("该帐号不存在！"), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 发送Code
        /// <summary>
        /// 发送Code
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendCode()
        {
            var code = "code".Form(string.Empty);
            var checkResult = VCodeHelper.Verify(code);
            if (!checkResult)
                return Json(DResult.Error("验证码错误！"), JsonRequestBehavior.AllowGet);

            var account = "account".Form(string.Empty);
            if (string.IsNullOrEmpty(account) || (!account.As<IRegex>().IsEmail() && !account.As<IRegex>().IsMobile()))
                return Json(DResult.Error("请输入正确的邮箱或者手机!"), JsonRequestBehavior.AllowGet);

            var exists = UserContract.ExistsAccount(account);
            if (!exists)
                return Json(DResult.Error("没有找到该注册帐号！"), JsonRequestBehavior.AllowGet);

            var dynamicCode = RandomHelper.RandomNums(6);//动态码
            var sessionKey = ValidateHelper.FindPwdDynamicCodeSession + "_" + account;
            var result = Helper.VCodeHelper.Send(account, dynamicCode, sessionKey, TemplateType.FindPwd);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 验证动态验证码
        /// <summary>
        /// 验证动态验证码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("check-dycode")]
        public ActionResult CheckDynamicCode()
        {
            var account = "account".Form("");
            var dyCode = "code".Form("");

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(dyCode))
                return Json(DResult.Error("参数错误，请稍后重试！"), JsonRequestBehavior.AllowGet);

            var sessionKey = ValidateHelper.FindPwdDynamicCodeSession + "_" + account;
            var ckCode = Helper.VCodeHelper.Check(account, dyCode, sessionKey);
            if (!ckCode.Status)
                return Json(ckCode, JsonRequestBehavior.AllowGet);

            SessionHelper.Add(ValidateHelper.CheckPassUserIdSession, account, 2);//存储帐号并记录经过了第一步

            return Json(DResult.Success, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}