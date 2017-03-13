using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Management;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [AdminAuthorize]
    public class HomeController : AdminController
    {
        public HomeController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }
    }
}