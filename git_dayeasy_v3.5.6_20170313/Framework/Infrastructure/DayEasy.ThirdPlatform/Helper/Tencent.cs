using System.Text;
using DayEasy.ThirdPlatform.Entity;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.ThirdPlatform.Entity.Result;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.ThirdPlatform.Helper
{
    /// <summary> 腾讯QQ登录 </summary>
    internal class Tencent : HelperBase
    {
        private string GetAccessToken(string code, string callBackUrl)
        {
            var url = Config.TokenUrl.FormatWith(Config.Partner, Config.Key, code, callBackUrl);
            string content = url.As<IHtml>().GetHtml(Encoding.UTF8);
            var val = PlatformUtility.GetContext(content);
            if (val["access_token"].IsNotNullOrEmpty())
                return val["access_token"];
            return string.Empty;
        }

        private string GetOpenId(string accessToken)
        {
            string url = Config.OpenIdUrl.FormatWith(accessToken);
            string content = url.As<IHtml>().GetHtml(Encoding.UTF8);
            return content.As<IRegex>().Match("\"openid\":\"(?<t>.*)?\"}", "t");
        }

        private string GetUserInfoString(string accessToken, string openId)
        {
            string url = Config.UserUrl.FormatWith(accessToken, Config.Partner, openId);
            return url.As<IHtml>().GetHtml(Encoding.UTF8);
        }

        protected override void Init()
        {
            LoadPlatform(PlatformType.Tencent);
            //            Callback = string.Format(Callback, PlatformType.Tencent.GetValue());
        }

        public override string LoginUrl(string host = null, string back = null)
        {
            return FormatLoginUrl(PlatformType.Tencent, host, back);
        }

        public override DResult<UserResult> User(string code = null)
        {
            code = string.IsNullOrWhiteSpace(code) ? "code".Query(string.Empty) : code;
            string accessToken = GetAccessToken(code, Callback);
            string openId = GetOpenId(accessToken);
            string userJson = GetUserInfoString(accessToken, openId);
            try
            {
                var result = userJson.JsonToObject<TencentResult>();
                if (result.ret == 0)
                {
                    var tResult = new UserResult
                    {
                        Profile = result.figureurl_qq_1,
                        Gender = result.gender,
                        Nick = result.nickname,
                        Id = openId,
                        AccessToken = accessToken
                    };
                    if (!string.IsNullOrWhiteSpace(result.figureurl_qq_2))
                        tResult.Profile = result.figureurl_qq_2;
                    return DResult.Succ(tResult);
                }
                return DResult.Error<UserResult>(result.msg);
            }
            catch
            {
                return DResult.Error<UserResult>("获取用户数据失败！");
            }
        }
    }
}
