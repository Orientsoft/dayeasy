using System;
using System.Web;
using Newtonsoft.Json;

namespace DayEasy.Web.File.ueditor
{
    /// <summary>
    /// Handler 的摘要说明
    /// </summary>
    public abstract class Handler
    {
        protected Handler(HttpContext context)
        {
            this.Request = context.Request;
            this.Response = context.Response;
            this.Context = context;
            this.Server = context.Server;
        }

        public abstract void Process();

        protected void WriteJson(object response)
        {
            Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With,X_Requested_With");

            string jsonpCallback = Request["callback"],
                json = JsonConvert.SerializeObject(response);
            if (String.IsNullOrWhiteSpace(jsonpCallback))
            {
                Response.AddHeader("Content-Type", "text/plain");
                Response.Write(json);
            }
            else 
            {
                Response.AddHeader("Content-Type", "application/javascript");
                Response.Write(String.Format("{0}({1});", jsonpCallback, json));
            }
            Response.End();
        }

        protected HttpRequest Request { get; private set; }
        private HttpResponse Response { get; set; }
        private HttpContext Context { get; set; }
        private HttpServerUtility Server { get; set; }
    }
}