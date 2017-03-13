using System.Text;
using DayEasy.ThirdPlatform.Entity;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.ThirdPlatform.Helper
{
    /// <summary> 腾讯微博登录 </summary>
    internal class TencentWeibo : HelperBase
    {
        private string GetAccessToken(string code, string callBackUrl)
        {
            string url = Config.TokenUrl.FormatWith(Config.Partner, Config.Key, callBackUrl, code);
            return url.As<IHtml>().GetHtml(Encoding.UTF8); //返回的不是单一accesstoken 带实体类。
        }

        protected override void Init()
        {
            LoadPlatform(PlatformType.TencentWeibo);
            Callback = string.Format(Callback, PlatformType.TencentWeibo.GetValue());
        }

        public override string LoginUrl(string host = null, string back = null)
        {
            return FormatLoginUrl(PlatformType.Tencent, host, back);
        }

        public override DResult<UserResult> User(string code = null)
        {
            code = string.IsNullOrWhiteSpace(code) ? "code".Query(string.Empty) : code;
            var context = GetAccessToken(code, Callback);

            var info = PlatformUtility.GetContext(context);
            if (info["errorCode"] == "0")
            {
                return DResult.Succ(new UserResult
                {
                    Id = info["openid"],
                    Nick = info["nick"]
                });
            }
            return DResult.Error<UserResult>(info["errorMsg"]);
        }
    }
}
