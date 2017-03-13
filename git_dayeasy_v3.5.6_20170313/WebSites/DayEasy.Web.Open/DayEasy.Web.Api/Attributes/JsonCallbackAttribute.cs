using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

namespace DayEasy.Web.Api.Attributes
{
    /// <summary> jsonp格式支持 </summary>
    public class JsonCallbackAttribute : ActionFilterAttribute
    {
        private const string CallbackQueryParameter = "callback";

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            string callback;

            if (IsJsonp(out callback))
            {
                var jsonBuilder = new StringBuilder(callback);
                var value = context.Response.Content.ReadAsStringAsync().Result;
                jsonBuilder.AppendFormat("({0})", value);
                context.Response.Content = new StringContent(jsonBuilder.ToString());
            }
            base.OnActionExecuted(context);
        }

        private bool IsJsonp(out string callback)
        {
            callback = HttpContext.Current.Request.QueryString[CallbackQueryParameter];
            return !string.IsNullOrEmpty(callback);
        }
    }
}
