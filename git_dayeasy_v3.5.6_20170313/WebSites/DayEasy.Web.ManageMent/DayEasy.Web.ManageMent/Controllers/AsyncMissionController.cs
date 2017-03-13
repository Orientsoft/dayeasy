using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility.Extend;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.ManageMent.Controllers
{
    /// <summary> 异步任务 </summary>
    [ManagerRoles(ManagerRole.LogManager)]
    [RoutePrefix("missions")]
    public class AsyncMissionController : AdminController
    {
        public AsyncMissionController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        [Route("")]
        public ActionResult Index(AsyncMissionSearchDto dto)
        {
            dto.Page = "pageIndex".Query(1) - 1;
            dto.Size = "pageSize".Query(15);
            var result = ManagementContract.AsyncMissions(dto.Type, dto.Status, dto.Keyword,
                DPage.NewPage(dto.Page, dto.Size));
            ViewData["typeList"] = MvcHelper.EnumToDropDownList<MissionType>(dto.Type, true, "所有类型");
            ViewData["statusList"] = MvcHelper.EnumToDropDownList<MissionStatus>(dto.Status, true, "所有状态");
            if (!result.Status)
            {
                ViewBag.Message = result.Message;
                return View();
            }
            ViewData["totalCount"] = result.TotalCount;
            return View(result.Data.ToList());
        }

        [HttpPost]
        [Route("reset")]
        public ActionResult Reset(string id)
        {
            var result = ManagementContract.ResetMission(id);
            return DJson.Json(result, true);
        }

        [HttpPost]
        [Route("update")]
        public ActionResult Update(string id, int priority)
        {
            var result = ManagementContract.UpdateMissionPriority(id, priority);
            return DJson.Json(result, true);
        }
    }
}