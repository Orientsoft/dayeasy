using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Portal.Controllers
{
    /// <summary> 帮助中心 </summary>
    [RoutePrefix("helper")]
    public class HelperController : DController
    {

        public HelperController(IUserContract userContract)
            : base(userContract) { }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("product-center")]
        public ActionResult ProductCenter()
        {
            return Redirect(Consts.Config.MainSite);
            //return View();
        }

        [Route("contact-us")]
        public ActionResult ContactUs()
        {
            return View();
        }

        [Route("questions")]
        public ActionResult Questions()
        {
            return Redirect(Consts.Config.MainSite);
            //return View();
        }

        [RoleAuthorize(UserRole.Teacher, "/work")]
        [Route("download")]
        public ActionResult DownLoad()
        {
            return View();
        }

        /// <summary>
        ///     阅卷工具下载
        /// </summary>
        /// <returns></returns>
        [RoleAuthorize(UserRole.Teacher, "/work")]
        [Route("download-markingtool")]
        public ActionResult DownMarkingTool()
        {
            try
            {
                var openUrl = Consts.Config.OpenSite + "download_url?type=" + 20;
                var fileUrlStream = HttpGet(openUrl);
                if (fileUrlStream != null)
                {
                    var fileUrl = Stream2String(fileUrlStream);
                    if (!string.IsNullOrEmpty(fileUrl))
                    {
                        fileUrl = fileUrl.Replace("\"", "");
                        var fileStream = HttpGet(fileUrl.Trim());
                        if (fileStream != null)
                        {
                            return File(fileStream, "application/octet-stream", "MarkingTool.exe");
                        }
                    }
                }

                return Content("");
            }
            catch (Exception)
            {
                return Content("");
            }
        }

        private string Stream2String(Stream stream)
        {
            var retString = string.Empty;
            if (stream != null)
            {
                var myStreamReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
            }
            return retString;
        }

        public Stream HttpGet(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            var response = (HttpWebResponse)request.GetResponse();
            var myResponseStream = response.GetResponseStream();
            return myResponseStream;
        }
    }
}