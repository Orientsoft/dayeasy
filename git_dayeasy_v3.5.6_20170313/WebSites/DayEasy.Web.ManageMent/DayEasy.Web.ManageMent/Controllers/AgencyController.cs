using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    /// <summary> 机构管理 </summary>
    [RoutePrefix("agency")]
    public class AgencyController : AdminController
    {
        public AgencyController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        public ActionResult Index(AgencySearchDto searchDto)
        {
            searchDto.Page = "pageIndex".Query(1) - 1;
            searchDto.Size = "pageSize".Query(15);
            ViewData["stages"] = MvcHelper.EnumToDropDownList<StageEnum>(searchDto.Stage, true, "所有学段");
            ViewData["levels"] = MvcHelper.EnumToDropDownList<CertificationLevel>(searchDto.Level, true, "所有");
            var result = ManagementContract.AgencySearch(searchDto);
            if (!result.Status)
                return MessageView(result.Message);
            var codes = result.Data.Select(a => a.AreaCode).Distinct().ToList();
            var areas = ManagementContract.GetAreas(codes);
            ViewData["areas"] = areas;
            ViewData["totalCount"] = result.TotalCount;
            return View(result.Data.ToList());
        }

        [AjaxOnly]
        [HttpPost]
        [Route("edit")]
        public ActionResult Edit(AgencyEditDto dto)
        {
            return DeyiJson(ManagementContract.EditAgency(dto));
        }

        [AjaxOnly]
        [HttpPost]
        [Route("certificate")]
        public ActionResult Certificate(string id)
        {
            return DeyiJson(ManagementContract.CertificateAgency(id), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("add")]
        public ActionResult Add(AgencyInputDto inputDto)
        {
            return DeyiJson(ManagementContract.AddAgency(inputDto), true);
        }
    }
}