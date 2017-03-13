using DayEasy.Utility.Extend;
using DayEasy.Utility.Logging;
using DayEasy.Web.File.ueditor;
using System;
using System.Linq;
using System.Web;

namespace DayEasy.Web.File.Handler
{
    public class UploadHandler : IHttpHandler
    {
        private readonly ILogger _logger = LogManager.Logger<UploadHandler>();
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request["action"])
            {
                case "config":
                    var action = new ConfigHandler(context);
                    action.Process();
                    break;
                case "uploadimage":
                    var action2 = new EditorUploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName")
                    });
                    action2.Process();
                    break;
                default:
                    NomalUpload(context);
                    break;
            }
        }

        private void NomalUpload(HttpContext context)
        {
            //查看原始数据
            //var data = new StringBuilder();
            //data.AppendLine(context.Request.Url.AbsoluteUri);
            //var stream = context.Request.InputStream;
            //stream.Seek(0, SeekOrigin.Begin);
            //using (var sr = new StreamReader(stream))
            //{
            //    data.AppendLine(sr.ReadToEnd());
            //    System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\updateFile.txt", data.ToString(),
            //        Encoding.UTF8);
            //}
            if (context.Request.HttpMethod.ToLower() != "post")
            {
                return;
            }

            var type = "type".QueryOrForm(0);
            try
            {
                var helper = new UploadHelper(context, (FileType)type);
                var result = helper.Save();
                if (!result.Status)
                {
                    Util.ResponseJson(new
                    {
                        state = 0,
                        msg = result.Message
                    }.ToJson());
                }
                else
                {
                    Util.ResponseJson(new
                    {
                        state = 1,
                        urls = result.Data.Values.ToArray(),
                        keys = context.Request.Files.AllKeys
                    }.ToJson());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                Util.ResponseJson(new
                {
                    state = 0,
                    msg = "上传失败"
                }.ToJson());
            }
            context.Response.End();
        }
    }
}