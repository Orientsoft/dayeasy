using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core;
using DayEasy.ThirdPlatform;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.Utility.Helper;

namespace DayEasy.Web.Account.Controllers
{
    /// <summary> 登录 </summary>
    [RoutePrefix("login")]
    public class LoginController : DController
    {
        public LoginController(IUserContract userContract)
            : base(userContract)
        {
        }

        #region Views

        [Route("")]
        public ActionResult Index()
        {
            UserContract.Logout();
            ViewData["User"] = null;
            return View();
        }

        [Route("popup")]
        public ActionResult Popup()
        {
            return View();
        }

        [Route("logout")]
        public ActionResult Logout(string return_url = "")
        {
            if (string.IsNullOrWhiteSpace(return_url))
                return_url = Consts.Config.MainSite;
            UserContract.Logout();
            return Redirect(return_url);
        }

        /// <summary>
        /// 生成第三方登录链接地址并跳转
        /// </summary>
        /// <param name="type"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("third-login")]
        public ActionResult ThirdLogin(int type, string host = null,string back = null)
        {
            var plat = PlatformFactory.GetInstance((PlatformType)type);
            var url = plat.LoginUrl(host, back);
            return Redirect(url);
        }

        [HttpGet]
        [Route("vcode")]
        public ActionResult Vcode()
        {
            var helper = new VCodeHelper(VCodeType.NumberAndLetter, 4);
            var bmp = helper.VCode();
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "image/gif");
        }

        #endregion

        #region Ajax操作

        [HttpPost]
        [Route("login")]
        public ActionResult Login(LoginDto user)
        {
            var result = UserContract.Login(user);
            if (result.Status)
            {
                CookieHelper.Delete(Consts.LoginCountCookieName);
            }
            else
            {
                var count = ConvertHelper.StrToInt(CookieHelper.GetValue(Consts.LoginCountCookieName), 0) + 1;
                CookieHelper.Set(Consts.LoginCountCookieName, count.ToString(), CookieHelper.GetHour(2));
            }
            return DJson.Json(result);
        }

        #endregion
    }
}
