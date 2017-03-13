using System.Text;
using DayEasy.ThirdPlatform.Entity;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.ThirdPlatform.Helper
{
    /// <summary> 微博登录 </summary>
    internal class Weibo : HelperBase
    {
        private string AccessToken(string code, string callBackUrl)
        {
            var url = Config.TokenUrl.FormatWith(Config.Partner, Config.Key, callBackUrl, code);
            string content = url.As<IHtml>().GetHtml("POST", string.Empty);
            return content;
        }

        protected override void Init()
        {
            LoadPlatform(PlatformType.Weibo);
            Callback = string.Format(Callback, PlatformType.Weibo.GetValue());
        }

        public override string LoginUrl(string host = null, string back = null)
        {
            return FormatLoginUrl(PlatformType.Tencent, host, back);
        }

        public override DResult<UserResult> User(string code = null)
        {
            code = string.IsNullOrWhiteSpace(code) ? "code".Query(string.Empty) : code;

            var accessToken = AccessToken(code, Callback);
            if (accessToken.IsNullOrEmpty())
                return DResult.Error<UserResult>("授权失败！");

            var val = PlatformUtility.GetContext(accessToken);
            if (!string.IsNullOrWhiteSpace(val["error"]))
                return DResult.Error<UserResult>(val["error"]);
            var info = new UserResult {Id = val["uid"]};
            var token = val["access_token"];
            var json = Config.UserUrl.FormatWith(info.Id, token).As<IHtml>().GetHtml(Encoding.UTF8);
            val = PlatformUtility.GetContext(json);
            info.Gender = (val["gender"] == "m" ? "男" : "女");
            info.Nick = val["name"];
            info.Profile = val["profile_image_url"];
            return DResult.Succ(info);
        }
    }
}
