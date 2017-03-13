using System.Web.Mvc;

namespace DayEasy.Web.Portal.Controllers
{
    /// <summary> 错误页 </summary>
    public class ErrorController : Controller
    {

        [Route("error/404")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("error/500")]
        public ActionResult Error500()
        {
            return View();
        }

        [Route("404")]
        public ActionResult NotFound()
        {
            return View("~/Views/Error/Index.cshtml");
        }


        [Route("500")]
        public ActionResult ServerError()
        {
            return View("~/Views/Error/Error500.cshtml");
        }
    }
}