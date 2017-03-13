using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.Manager)]
    [RoutePrefix("user/manager")]
    public class ManagerController : AdminController
    {
        public ManagerController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        [Route("")]
        public ActionResult Index(ManagerSearchDto searchDto)
        {
            searchDto.Page = "pageIndex".Query(1) - 1;
            searchDto.Size = "pageSize".Query(15);
            var result = ManagementContract.ManagerSearch(searchDto);
            if (!result.Status)
                return Content(result.Message);
            ViewData["totalCount"] = result.TotalCount;
            return View(result.Data.ToList());
        }

        /// <summary> 设置管理员 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("set")]
        public ActionResult Set(string keyword)
        {
            var result = ManagementContract.SetManager(keyword);
            return DeyiJson(result, true);
        }
        /// <summary> 移除管理员身份 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("remove")]
        public ActionResult Remove(long id)
        {
            var result = ManagementContract.RemoveManager(id);
            return DeyiJson(result, true);
        }

        /// <summary> 修改管理员权限 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("update")]
        public ActionResult Update(long id, long role)
        {
            var result = ManagementContract.UpdateManager(id, role);
            return DeyiJson(result, true);
        }
    }
}