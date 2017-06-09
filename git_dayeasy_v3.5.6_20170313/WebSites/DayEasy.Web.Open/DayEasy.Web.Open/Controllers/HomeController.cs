using DayEasy.Contract.Open.Contracts;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Core;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Timing;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;
using DayEasy.Web.Api.Config;
using System;
using System.Web;
using System.Web.Http;

namespace DayEasy.Web.Open.Controllers
{
    public class HomeController : ApiController
    {
        private readonly IPaperContract _paperContract;
        private readonly IOpenContract _openContract;
        public HomeController(IPaperContract paperContract, IOpenContract openContract)
        {
            _paperContract = paperContract;
            _openContract = openContract;
        }

        [JsonCallback]
        [HttpGet]
        public QuestionDto Load(string id)
        {
            return _paperContract.LoadQuestion(id);
        }

        [HttpGet]
        [Route("~/ticks")]
        public long Ticks()
        {
            //return Clock.Now.MilliTimespan();
            return ApiExtends.ToLong(Clock.Now);
        }

        [HttpGet]
        [Route("~/download_url")]
        public string DownloadUrl(int type)
        {
            var manifest = ManifestInfo.Get(type);
            if (manifest != null)
                return manifest.DownloadUrl;
            return string.Empty;
        }

        [HttpGet]
        [Route("~/download")]
        public DResult Download(int type)
        {
            var manifest = ManifestInfo.Get(type);
            if (manifest != null)
            {
                HttpContext.Current.Response.Redirect(manifest.DownloadUrl, true);
            }
            return DResult.Success;
        }

        [HttpGet]
        [Route("~/auto_login")]
        public void AutoLogin(string partner, string account, long tick, string sign)
        {
            var result = DResult.Success;
            if (Clock.Now >= tick.FromMillisecondTimestamp().AddMinutes(5))
                result = DResult.Error("请求已超时");
            var key = PartnerBusi.Instance.GetKey(partner);
            if (result.Status)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    result = DResult.Error("合作商户不存在");
                }
            }
            if (result.Status)
            {
                var currentSign = $"account={account}&tick={tick}+{key}".Md5();
                if (!string.Equals(currentSign, sign, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = DResult.Error("签名验证失败");
                }
            }
            if (result.Status)
            {
                result = _openContract.AutoLogin(account);
            }
            if (result.Status)
            {
                HttpContext.Current.Response.Redirect(Consts.Config.MainSite, true);
                return;
            }
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(result.ToJson());
            HttpContext.Current.Response.End();
        }
    }
}
