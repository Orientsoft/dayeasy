using System.Collections.Specialized;
using System.Text;
using DayEasy.ThirdPlatform.Entity;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.ThirdPlatform.Helper
{
    /// <summary> 微信登录 </summary>
    internal class Weixin : HelperBase
    {
        private NameValueCollection AccessToken(string code)
        {
            var url = Config.TokenUrl.FormatWith(Config.Partner, Config.Key, code);
            string content = url.As<IHtml>().GetHtml(Encoding.UTF8);
            return PlatformUtility.GetContext(content);
        }

        protected override void Init()
        {
            LoadPlatform(PlatformType.Weixin);
            Callback = string.Format(Callback, PlatformType.Weixin.GetValue());
        }

        public override string LoginUrl(string host = null, string back = null)
        {
            return FormatLoginUrl(PlatformType.Tencent, host, back);
        }

        public override DResult<UserResult> User(string code = null)
        {
            code = string.IsNullOrWhiteSpace(code) ? "code".Query(string.Empty) : code;
            var col = AccessToken(code);
            if (string.IsNullOrWhiteSpace(col["access_token"]))
                return DResult.Error<UserResult>("授权失败！");
            var result = new UserResult
            {
                Id = col["openid"],
                AccessToken = col["access_token"]
            };
            var url = string.Format(Config.UserUrl, result.AccessToken, result.Id);
            var html = url.As<IHtml>().GetHtml(Encoding.UTF8);
            var userCollect = PlatformUtility.GetContext(html);
            if (!string.IsNullOrWhiteSpace(userCollect["errcode"]))
                return DResult.Error<UserResult>(userCollect["errmsg"]);
            result.Nick = userCollect["nickname"];
            result.Gender = (userCollect["sex"] == "1" ? "男" : "女");
            result.Profile = userCollect["headimgurl"];
            return DResult.Succ(result);
        }
    }
}
