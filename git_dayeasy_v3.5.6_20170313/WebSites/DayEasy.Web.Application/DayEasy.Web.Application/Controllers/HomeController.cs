using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Application.Controllers
{
    [DAuthorize]
    public class HomeController : DController
    {
        public HomeController(IUserContract userContract)
            : base(userContract)
        {
        }

        [Route("~/")]
        public ActionResult Index()
        {
            return View();
        }
    }
}