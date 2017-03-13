using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Utility.Helper;
using DayEasy.Web;

namespace DayEasy.MvcSample.Controllers
{
    public class UserController : DController
    {

        public UserController(IUserContract userContract) : base(userContract){}

        public ActionResult Index()
        {
            const long userId = 304533611023;
            var user = UserContract.Load(userId);
            return View(user);
        }

        [Route("~/vcode/{type=0}")]
        public ActionResult Vcode(int type)
        {
            var codeType = (VCodeType)type;
            var helper = new VCodeHelper(codeType, 4);
            int width = 22;
            if (codeType == VCodeType.ChineseWord)
                width = 30;
            var bmp = helper.VCode(width);
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "image/png");
        }

        [Route("~/verify/{code}")]
        public bool Verify(string code)
        {
            return VCodeHelper.Verify(code);
        }
    }
}