using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using DayEasy.Utility;
using DayEasy.Utility.Extend;

namespace DayEasy.Web.Api.Attributes
{
    /// <summary> 接口签名过滤 </summary>
    public class DApiAttribute : AuthorizeAttribute
    {
        private string _errorMsg;
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var context = (HttpContextBase)actionContext.Request.Properties["MS_HttpContext"];
            var ps = (string.Equals(context.Request.HttpMethod, "post", StringComparison.CurrentCultureIgnoreCase)
                ? context.Request.Form
                : context.Request.QueryString);
            var partner = ps["partner"] ?? string.Empty;
            var sign = ps["sign"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(partner) || string.IsNullOrWhiteSpace(sign))
            {
                _errorMsg = "接口请求失败，没有合作者帐号或签名！";
                return false;
            }
            var mySign = PartnerBusi.Instance.SignPartner(partner, ps);
            if (string.IsNullOrWhiteSpace(mySign) || mySign != ps["sign"])
            {
                _errorMsg = "签名无效，接口授权失败！";
                return false;
            }
            if (!CheckTick(ps["tick"].CastTo(0L)))
            {
                _errorMsg = "请求已过时~！";
                return false;
            }

            return true;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK,
                DResult.Error(_errorMsg));
        }

        /// <summary> 时效性验证(5分钟) </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        private bool CheckTick(long tick)
        {
            if (tick <= 0) return false;
            return tick.TimeSpan().TotalMinutes < 5;
        }
    }
}
