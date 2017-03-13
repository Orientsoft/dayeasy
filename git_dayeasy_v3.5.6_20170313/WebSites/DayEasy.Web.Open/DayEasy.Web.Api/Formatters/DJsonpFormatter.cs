using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Serialization;

namespace DayEasy.Web.Api.Formatters
{
    public class DJsonpFormatter : JsonMediaTypeFormatter
    {
        public DJsonpFormatter()
        {
            SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        private string _callbackQueryParameter;

        private string CallbackQueryParameter
        {
            get { return _callbackQueryParameter ?? "callback"; }
            set { _callbackQueryParameter = value; }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content,
            TransportContext transportContext)
        {
            string callback;

            if (IsJsonpRequest(out callback))
            {
                return Task.Factory.StartNew(() =>
                {
                    var jsonBuilder = new StringBuilder(callback);
                    jsonBuilder.AppendFormat("({0})", value);

                    base.WriteToStreamAsync(type, jsonBuilder, stream, content, transportContext).Wait();
                });
            }
            return base.WriteToStreamAsync(type, value, stream, content, transportContext);
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, Encoding effectiveEncoding)
        {
            string callback;

            if (IsJsonpRequest(out callback))
            {
                var jsonBuilder = new StringBuilder(callback);
                jsonBuilder.AppendFormat("({0})", value);
                var streamWriter = new StreamWriter(writeStream, effectiveEncoding);
                streamWriter.Write(jsonBuilder.ToString());
                streamWriter.Flush();
            }
        }

        /// <summary>
        /// 判断是否为跨域请求
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private bool IsJsonpRequest(out string callback)
        {
            callback = null;

            if (HttpContext.Current.Request.HttpMethod != "GET")
                return false;

            callback = HttpContext.Current.Request.QueryString[CallbackQueryParameter];

            return !string.IsNullOrEmpty(callback);
        }
    }
}
