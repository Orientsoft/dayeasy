using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace DayEasy.Web.ManageMent.Controllers
{
    [RoutePrefix("system")]
    public class SystemController : AdminController
    {
        public SystemController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        [HttpGet]
        [Route("download-logs")]
        [ManagerRoles(ManagerRole.SystemManager)]
        public ActionResult DownloadLogs(int type = -1, string keyword = null, int pageindex = 1, int pagesize = 15)
        {
            var result = ManagementContract.DownloadLogs(type, keyword, DPage.NewPage(pageindex - 1, pagesize));
            if (result.Status && result.Data != null)
            {
                var userIds = result.Data.Select(u => u.UserId).Distinct().ToList();
                var userDict = UserContract.LoadList(userIds).ToDictionary(k => k.Id, v => v);
                ViewData["userDict"] = userDict;
            }
            ViewData["types"] = MvcHelper.EnumToDropDownList<DownloadType>(type, true, "所有类型");
            ViewBag.Total = result.TotalCount;
            return View(result.Data.ToList());
        }

        [AjaxOnly]
        [HttpPost]
        [Route("agency")]
        public ActionResult Agency(string id)
        {
            var agency = ManagementContract.Agency(id);
            if (agency == null)
                return DeyiJson(DResult.Error("机构不存在！"));
            return DeyiJson(DResult.Succ(agency));
        }

        [AjaxOnly]
        [HttpPost]
        [Route("agencies")]
        public ActionResult AgencySearch(string keyword)
        {
            keyword = keyword.UrlDecode(Encoding.UTF8);
            return DeyiJson(ManagementContract.AgencySearch(keyword));
        }

        [AjaxOnly]
        [HttpGet]
        [Route("areas")]
        public ActionResult Areas(int code = 0)
        {
            var systemContract = CurrentIocManager.Resolve<ISystemContract>();
            var areas = systemContract.Areas(code).OrderBy(t => t.Sort).Select(t => new { t.Id, t.Name }).ToList();
            return DeyiJson(areas);
        }
    }
}