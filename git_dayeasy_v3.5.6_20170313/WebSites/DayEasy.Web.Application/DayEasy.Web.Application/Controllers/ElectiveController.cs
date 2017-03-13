using DayEasy.Application.Services.Helper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Elective;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 选课应用 </summary>
    public class ElectiveController : DController
    {
        private readonly IElectiveContract _electiveContract;
        private const string AppName = "教务管理";
        public ElectiveController(IUserContract userContract, IElectiveContract electiveContract)
            : base(userContract)
        {
            _electiveContract = electiveContract;
            ViewBag.AppName = AppName;
        }

        #region Views
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Index(int pageIndex = 1, int pageSize = 10)
        {
            var agencyResult = UserContract.ApplicationAgency(UserId, 1012);
            if (!agencyResult.Status)
                return MessageView(agencyResult.Message);
            var result = _electiveContract.List(agencyResult.Data.Id, DPage.NewPage(pageIndex - 1, pageSize));
            ViewBag.TotalCount = result.TotalCount;
            var list = (result.Data ?? new List<ElectiveBatchDto>()).ToList();
            return View(list);
        }

        [HttpGet]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Detail(string batch)
        {
            var result = _electiveContract.CourseList(batch);
            if (!result.Status)
                return MessageView(result.Message);
            return View(result.Data);
        }

        [HttpGet]
        [RoleAuthorize(UserRole.Student, "/")]
        public ActionResult Item(string batch)
        {
            ViewBag.Batch = batch;
            return View();
        }

        [HttpPost]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Import()
        {
            var name = "name".Form(string.Empty);
            var fileData = "fileData".Form(string.Empty);
            var list = ElectiveHelper.Import(name, fileData);
            return DeyiJson(list, true);
        }

        [RoleAuthorize(UserRole.Teacher, "/")]
        public void Export(string batch, string title)
        {
            var result = _electiveContract.Details(batch);
            if (!result.Status)
                CurrentContext.Response.Write(result.Message);
            ElectiveHelper.Export(title, result.Data.ToList());
        }

        [HttpGet]
        public void Template()
        {
            ElectiveHelper.DownloadTemplete();
        }
        #endregion

        #region Ajax

        [HttpGet]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult ClassList()
        {
            var agencyResult = UserContract.ApplicationAgency(UserId, 1012);
            if (!agencyResult.Status)
                return DeyiJson(agencyResult);
            var groupContract = CurrentIocManager.Resolve<IGroupContract>();
            var groupResult = groupContract.SearchGroups(new SearchGroupDto
            {
                AgencyId = agencyResult.Data.Id,
                CertificationLevels = new byte?[] { (byte)CertificationLevel.Official },
                Types = new List<int> { (int)GroupType.Class }
            });
            if (!groupResult.Status || groupResult.Data == null)
                return DeyiJson(groupResult);
            var groups = groupResult.Data.Select(t => t as ClassGroupDto)
                .ToList();
            var gradeGroups = groups.GroupBy(t => t.GradeYear)
                .OrderByDescending(t => t.Key)
                .ToDictionary(k => k.Key, v => v.Select(t => new
                {
                    t.Id,
                    t.Name
                }).OrderBy(t => t.Name.ClassIndex()).ToList());
            return DeyiJson(gradeGroups);
        }

        [HttpPost]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Create(string data)
        {
            var agencyResult = UserContract.ApplicationAgency(UserId, 1012);
            if (!agencyResult.Status)
                return DeyiJson(agencyResult);
            var dto = JsonHelper.Json<ElectiveInputDto>(data.UrlDecode());
            dto.UserId = UserId;
            dto.AgencyId = agencyResult.Data.Id;
            var result = _electiveContract.Create(dto);
            return DeyiJson(result);
        }

        [HttpPost]
        [AjaxOnly]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Start(string batch)
        {
            var result = _electiveContract.Start(batch);
            return DeyiJson(result);
        }

        [HttpPost]
        [AjaxOnly]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Close(string batch)
        {
            var result = _electiveContract.Close(batch);
            return DeyiJson(result);
        }

        [HttpPost]
        [AjaxOnly]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult Delete(string batch)
        {
            var result = _electiveContract.Delete(batch);
            return DeyiJson(result);
        }

        [HttpPost]
        [AjaxOnly]
        [DAuthorize]
        public ActionResult Course(int id)
        {
            var result = _electiveContract.Course(id, ChildOrUserId);
            return DeyiJson(result);
        }

        [HttpPost]
        [AjaxOnly]
        [DAuthorize]
        public ActionResult Quit(int id)
        {
            var result = _electiveContract.QuitCourse(id, ChildOrUserId);
            return DeyiJson(result);
        }

        [HttpGet]
        public ActionResult CourseList(string batch)
        {
            var result = _electiveContract.CourseList(batch, ChildOrUserId);
            return DeyiJson(result);
        }

        public ActionResult CourseBatch(string agencyId, string callback = null)
        {
            var batch = _electiveContract.AgencyCourse(agencyId, ChildOrUserId);
            return new JsonpResult(batch, callback);
        }

        #endregion
    }
}