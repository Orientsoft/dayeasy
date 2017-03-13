using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.LogManager)]
    [RoutePrefix("sys/logs")]
    public class LogController : AdminController
    {
        public LogController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        [Route("")]
        public ActionResult Index(int status = 0, int pageIndex = 1, int pageSize = 15)
        {
            var result = ManagementContract.Logs(status, pageIndex - 1, pageSize);
            ViewData["totalCount"] = result.TotalCount;
            return View(result.Data.ToList());
        }

        [Route("resolve")]
        public ActionResult Resolve(string id)
        {
            return DeyiJson(ManagementContract.ResolveLog(id));
        }
    }
}