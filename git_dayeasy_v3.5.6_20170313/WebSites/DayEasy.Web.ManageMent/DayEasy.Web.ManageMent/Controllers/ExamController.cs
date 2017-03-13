using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Core.Domain;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.ManageMent.Controllers
{
    /// <summary> 大型考试 </summary>
    [RoutePrefix("exam")]
    [ManagerRoles(ManagerRole.OperationManager)]
    public class ExamController : AdminController
    {
        private readonly IExaminationContract _examinationContract;
        public ExamController(IUserContract userContract, IManagementContract managementContract, IExaminationContract examinationContract)
            : base(userContract, managementContract)
        {
            _examinationContract = examinationContract;
        }

        [HttpGet]
        public ActionResult Index(int status = -1, int pageindex = 1, int pagesize = 15)
        {
            ViewData["status"] = MvcHelper.EnumToDropDownList<ExamStatus>(status, true, "所有状态");
            var result = _examinationContract.Examinations(status, page: DPage.NewPage(pageindex - 1, pagesize));
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.TotalCount = result.TotalCount;
            return View(result.Data.ToList());
        }

        [HttpGet]
        [Route("create")]
        public ActionResult Create(string agencyId, int pageindex = 1, int pagesize = 10)
        {
            //if (string.IsNullOrWhiteSpace(agencyId))
            //{
            //    ViewBag.TotalCount = 0;
            //    return View(new List<ExamSubjectDto>());
            //}
            var result = _examinationContract.JointList(new JointSearchDto
            {
                AgencyId = agencyId,
                Page = pageindex - 1,
                Size = pagesize
            });
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.TotalCount = result.TotalCount;
            return View(result.Data.ToList());
        }

        [HttpGet]
        [Route("unions")]
        public ActionResult Unions(int pageindex = 1, int pagesize = 8)
        {
            var result = _examinationContract.UnionList(DPage.NewPage(pageindex - 1, pagesize));
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.TotalCount = result.TotalCount;
            return View(result.Data.ToList());
        }

        [HttpGet]
        [Route("create-union")]
        public ActionResult CreateUnion(int pageindex = 1, int pagesize = 15)
        {
            var result = _examinationContract.Examinations((int)ExamStatus.Sended, null,
                DPage.NewPage(pageindex - 1, pagesize), false);
            if (!result.Status)
                return MessageView(result.Message);
            ViewBag.TotalCount = result.TotalCount;
            return View(result.Data.ToList());
        }

        [HttpPost]
        [Route("create")]
        public ActionResult Create(ExamDto dto)
        {
            dto.CreatorId = UserId;
            var result = _examinationContract.CreateExamination(dto);
            return DeyiJson(result, true);
        }

        [HttpPost]
        [Route("delete")]
        public ActionResult Delete(string id)
        {
            var result = _examinationContract.DeleteExamination(id, UserId);
            return DeyiJson(result, true);
        }

        [HttpPost]
        [Route("send")]
        public ActionResult Send(string id)
        {
            var result = _examinationContract.SendExamination(id, UserId);
            return DeyiJson(result, true);
        }

        [HttpGet]
        [Route("details")]
        public ActionResult ExamDetails(string id)
        {
            var result = _examinationContract.ExamJointList(id);
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("cancel-union")]
        public ActionResult CancelUnion(string unionBatch)
        {
            return DeyiJson(_examinationContract.CancelUnion(unionBatch), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("add-union")]
        public ActionResult AddUnion(string[] examIds)
        {
            return DeyiJson(_examinationContract.UnionReport(examIds.ToList(), UserId), true);
        }
    }
}