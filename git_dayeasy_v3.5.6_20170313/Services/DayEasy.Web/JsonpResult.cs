using System.Web.Mvc;
using DayEasy.Utility.Helper;

namespace DayEasy.Web
{
    /// <summary> jsonp ActionResult </summary>
    public class JsonpResult : ActionResult
    {
        private readonly object _result;
        private readonly string _callback;

        public JsonpResult(object result, string callback = null)
        {
            _result = result;
            _callback = string.IsNullOrWhiteSpace(callback) ? "callback" : callback;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var jsonp = _callback + "(" + JsonHelper.ToJson(_result, NamingType.CamelCase) + ")";
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.Write(jsonp);
            context.HttpContext.Response.End();
        }
    }
}
