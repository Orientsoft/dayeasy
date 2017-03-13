using System.Web.Mvc;

namespace DayEasy.Web.Portal.Controllers
{
    [RoutePrefix("act")]
    public class ActivityController : Controller
    {
        [Route("poster")]
        public ActionResult Poster()
        {
            return View();
        }
    }
}