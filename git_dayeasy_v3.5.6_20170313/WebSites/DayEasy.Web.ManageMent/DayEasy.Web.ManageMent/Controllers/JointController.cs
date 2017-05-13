using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Management.Services.Helper;
using DayEasy.Office;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using JointMarkingDto = DayEasy.Contracts.Management.Dto.JointMarkingDto;

namespace DayEasy.Web.ManageMent.Controllers
{
    [ManagerRoles(ManagerRole.OperationManager)]
    [RoutePrefix("joint")]
    public class JointController : AdminController
    {
        public JointController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        #region Views
        [Route("")]
        public ActionResult Index(JointSearchDto searchDto)
        {
            searchDto.Page = "pageIndex".Query(1) - 1;
            searchDto.Size = "pageSize".Query(15);
            var subjects = new List<SelectListItem>
            {
                new SelectListItem{Value = "-1",Text = "所有科目",Selected = searchDto.SubjectId<=0}
            };
            subjects.AddRange(SystemCache.Instance.Subjects().Select(t => new SelectListItem
            {
                Text = t.Value,
                Value = t.Key.ToString(),
                Selected = searchDto.SubjectId == t.Key
            }));
            ViewData["subjects"] = subjects;
            ViewData["status"] = MvcHelper.EnumToDropDownList<JointStatus>(searchDto.Status, true, "所有状态");
            var result = ManagementContract.JointList(searchDto);
            ViewBag.TotalCount = result.TotalCount;
            if (!result.Status)
                return View(new List<JointMarkingDto>());
            return View(result.Data.ToList());
        }

        [Route("~/marking/list")]
        public ActionResult MarkingList(VMarkingInputDto searchDto)
        {
            searchDto.Page = "pageIndex".Query(1) - 1;
            searchDto.Size = "pageSize".Query(15);
            var subjects = new List<SelectListItem>
            {
                new SelectListItem{Value = "-1",Text = "所有科目",Selected = searchDto.SubjectId<=0}
            };
            subjects.AddRange(SystemCache.Instance.Subjects().Select(t => new SelectListItem
            {
                Text = t.Value,
                Value = t.Key.ToString(),
                Selected = searchDto.SubjectId == t.Key
            }));
            ViewData["subjects"] = subjects;
            ViewData["status"] = MvcHelper.EnumToDropDownList<MarkingStatus>(searchDto.MarkingStatus, true, "所有状态");
            var result = ManagementContract.MarkingList(searchDto);
            ViewBag.TotalCount = result.TotalCount;
            return View((result.Data ?? new List<VMarkingDto>()).ToList());
        }

        [Route("exceptions/{joint}")]
        public ActionResult Exceptions(string joint, int status = -1, int pageindex = 1, int pagesize = 15, int type = -1)
        {
            byte? exType = null;
            if (type > -1 && type < 5) exType = (byte)type;
            var result = ManagementContract.JointExceptions(joint, status, DPage.NewPage(pageindex - 1, pagesize), exType);
            if (!result.Status)
                return MessageView(result.Message);
            ViewData["status"] = MvcHelper.EnumToDropDownList<JointExceptionStatus>(status, true, "所有状态");
            ViewData["type"] = MvcHelper.EnumToDropDownList<MarkingExceptionType>(type, true, "所有类型");
            ViewBag.TotalCount = result.TotalCount;
            return View(result.Data.ToList());
        }

        [Route("picture/{id}")]
        public ActionResult Picture(string id)
        {
            var picture = ManagementContract.Picture(id);
            if (picture == null)
                return MessageView("试卷图片不存在！");
            return View("~/Views/Joint/Pictures.cshtml", new List<DKeyValue> { picture });
        }
        [AllowAnonymous]
        [Route("pictures/{joint}/{type}")]
        public ActionResult Pictures(string joint, int type)
        {
            var pictures = ManagementContract.JointPictures(joint, (byte)type);
            return View(pictures);
        }

        [Route("~/marking/pictures/{batch}/{type}")]
        public ActionResult MarkingPictures(string batch, int type)
        {
            var pictures = ManagementContract.MarkingPictures(batch, (byte)type);
            return View("Pictures", pictures);
        }

        #endregion

        /// <summary> 导出协同数据 </summary>
        /// <param name="jointBatch"></param>
        [Route("export/{jointBatch}")]
        public void ExportJoint(string jointBatch)
        {
            ManagementContract.ExportJoint(jointBatch);
        }

        /// <summary> 导出考试数据 </summary>
        /// <param name="batch"></param>
        [Route("~/marking/export/{batch}")]
        public void ExportMarking(string batch)
        {
            ManagementContract.ExportMarking(batch);
        }

        #region Ajax

        [AjaxOnly]
        [HttpPost]
        [Route("recall")]
        public ActionResult Recall(string jointBatch)
        {
            return DeyiJson(ManagementContract.JointRecall(jointBatch), true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("~/marking/recall")]
        public ActionResult MarkingRecall(string batch)
        {
            return DeyiJson(ManagementContract.Recall(batch), true);
        }

        [Route("un-submits")]
        public ActionResult UnSubmits(string jointBatch)
        {
            return DeyiJson(ManagementContract.JointUnsubmits(jointBatch));
        }

        [HttpPost]
        [Route("solve-exception")]
        public ActionResult SolveException(string id)
        {
            return DeyiJson(ManagementContract.SolveJointException(id));
        }

        [HttpPost]
        [Route("reset")]
        public ActionResult ResetJoint(string jointBatch)
        {
            return DeyiJson(ManagementContract.ResetJoint(jointBatch, UserId));
        }

        [HttpPost]
        [Route("import-data")]
        public ActionResult ImportFile(string jointBatch)
        {
            const string method = "top.window.importCallback";
            var file = Request.Files.Get(0);
            if (file == null)
                return new ScriptResult(DResult.Error("未上传任何文件"), method);
            var ext = Path.GetExtension(file.FileName);
            if (!string.Equals(ext, ".xls", System.StringComparison.CurrentCultureIgnoreCase))
                return new ScriptResult(DResult.Error("只支持xls格式文件"), method);
            var ds = ExcelHelper.Read(file.InputStream);
            var dtos = ds.ParseJData();
            if (dtos == null || !dtos.Any())
                return new ScriptResult(DResult.Error("没有任何数据"), method);
            var result = CurrentIocManager.Resolve<IMarkingContract>().ImportJointData(jointBatch, dtos);
            return new ScriptResult(result, "top.window.importCallback");
            //return DeyiJson(DResult.Succ(file));
        }

        [HttpPost]
        [Route("complete-joint")]
        public ActionResult CompleteJoint(string jointBatch)
        {
            var inputDto = new CompleteMarkingInputDto
            {
                Batch = jointBatch,
                IsJoint = true,
                SetIcon = false,
                SetMarks = false,
                UserId = UserId
            };
            return DeyiJson(CurrentIocManager.Resolve<IMarkingContract>().CompleteMarking(inputDto), true);
        }
        #endregion
    }
}