using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Config;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 接口文档 </summary>
    [AllowAnonymous]
    [RoutePrefix("doc")]
    public class DocumentController : ApiController
    {
        /// <summary> 获取接口分类 </summary>
        [Route("category")]
        [HttpGet]
        public object Category()
        {
            var doc = ConfigUtils<ApiDocInfo>.Instance.Get();
            return doc.Cates.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                methods = t.Methods.Select(m => new
                {
                    id = m.Id,
                    name = m.Name
                })
            });
        }

        [Route("methods")]
        [HttpGet]
        public object Methods(int cid)
        {
            var doc = ConfigUtils<ApiDocInfo>.Instance.Get();
            var method = doc.Cates.FirstOrDefault(t => t.Id == cid);
            if (method != null)
                return method.Methods.Select(t => new
                {
                    id = t.Id,
                    name = t.Name
                });
            return new { };
        }

        [Route("codes")]
        [HttpGet]
        public object StatusCodes()
        {
            var codes = ConfigUtils<ApiDocInfo>.Instance.Get().StatusCodes;
            return codes.Select(t => new
            {
                code = t.Code,
                type = t.Type == 0 ? "系统" : "用户",
                msg = t.Msg,
                desc = t.Description
            });
        }

        /// <summary> 获取接口方法 </summary>
        [Route("method")]
        [HttpGet]
        public ApiMethodInfo Method(int id)
        {
            var doc = ConfigUtils<ApiDocInfo>.Instance.Get();
            var method = doc.Cates.SelectMany(t => t.Methods).FirstOrDefault(t => t.Id == id);
            if (method != null)
            {
                method.Url = (string.IsNullOrWhiteSpace(method.Url) ? doc.Url : method.Url);
                method.DataType = (string.IsNullOrWhiteSpace(method.DataType) ? doc.DataType : method.DataType);
                method.Type = (string.IsNullOrWhiteSpace(method.Type) ? doc.Method : method.Type);
                return method;
            }
            return new ApiMethodInfo();
        }

        /// <summary> 执行接口 </summary>
        [Route("execute")]
        [HttpPost]
        public object Execute()
        {
            return ExecuteMethod();
        }

        private object ExecuteMethod()
        {
            string method = "method".Form(""),
                type = "type".Form("GET"),
                key = "key".Form(""),
                secret = "secret".Form(""),
                token = "token".Form(""),
                parms = "parms".Form("");
            var req = HttpContext.Current.Request;
            var url = "http://" + req.ServerVariables["HTTP_HOST"] + "/v3/{0}";
            url = url.FormatWith(method.Replace(".", "_"));
            type = type.ToUpper();
            parms = "partner={0}".FormatWith(key) + (string.IsNullOrEmpty(parms) ? "" : "&" + parms);
            if (!string.IsNullOrWhiteSpace(token))
            {
                parms += "&token={0}".FormatWith(token);
            }
            parms += "&tick={0}".FormatWith(Clock.Now.ToLong());
            var secretValue = PartnerBusi.Instance.GetKey(key);
            if (secretValue == secret)
            {
                parms = PartnerBusi.Instance.SignPartner(parms);
            }
            var watch = new Stopwatch();
            string result, currentUrl = url + "?" + parms;
            watch.Start();
            if (type.ToUpper() == "POST")
            {
                using (var http = new HttpHelper(url, type, Encoding.UTF8, parms))
                {
                    //http.SetContentType("application/json");
                    result = http.GetHtml();
                    watch.Stop();
                }
            }
            else
            {
                using (var http = new HttpHelper(currentUrl, Encoding.UTF8))
                {
                    http.SetContentType("application/json");
                    result = http.GetHtml();
                    watch.Stop();
                }
            }

            return new { url = currentUrl, result, time = (watch.ElapsedTicks / 10000F).ToString("N") };
        }
    }
}
