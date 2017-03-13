using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Management.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Management.Services.Helper;
using DayEasy.Web.Filters;
using DayEasy.Web.ManageMent.Common;
using DayEasy.Web.ManageMent.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Services.Helper;

namespace DayEasy.Web.ManageMent.Controllers
{
    /// <summary> 知识点管理 </summary>
    [RoutePrefix("sys/kps")]
    [ManagerRoles(ManagerRole.SystemManager)]
    public class KpController : AdminController
    {
        public KpController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        #region Views
        /// <summary> 知识点列表 </summary>
        [Route("")]
        public ActionResult Index(KnowledgeSearchDto searchDto)
        {
            ViewData["stages"] = MvcHelper.EnumToDropDownList<StageEnum>(searchDto.Stage, true, "选择学段");
            var subjects = SystemCache.Instance.Subjects();
            ViewData["subjects"] = subjects.ToSlectListItemList(s => s.Value, s => s.Key, searchDto.SubjectId, true,
                "选择科目");
            var result = ManagementContract.KnowledgeSearch(searchDto);
            if (!string.IsNullOrWhiteSpace(searchDto.Keyword) && result.Status && result.Data.Any())
            {
                var paths = new Dictionary<int, Dictionary<int, string>>();
                foreach (var item in result.Data)
                {
                    paths.Add(item.Id, ManagementContract.KnowledgePath(item.PID));
                }
                ViewData["paths"] = paths;
            }
            if (searchDto.ParentId > 0)
            {
                ViewData["parents"] = ManagementContract.KnowledgePath(searchDto.ParentId);
            }
            return View((result.Data ?? new List<TS_Knowledge>()).ToList());
        }

        [HttpGet]
        [Route("move-list")]
        public ActionResult MoveList(int pageindex = 1, int pagesize = 15)
        {
            var result = KnowledgeMover.Instance.MoveList(-1, string.Empty, pageindex - 1, pagesize);
            if (!result.Status)
                return MessageView(result.Message);
            ViewData["totalCount"] = result.TotalCount;
            return View(result.Data.ToList());
        }

        #endregion


        #region Ajax
        /// <summary> 修改知识点 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("edit")]
        public ActionResult Edit(KnowledgeDto dto)
        {
            var result = ManagementContract.KnowledgeUpdate(dto);
            return DeyiJson(result, true);
        }

        /// <summary> 删除 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("update-status")]
        public ActionResult UpdateStatus(int knowledgeId, int status)
        {
            var result = ManagementContract.KnowledgeUpdateStatus(knowledgeId, status);
            return DeyiJson(result, true);
        }

        /// <summary> 添加知识点 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("add")]
        public ActionResult Add(KnowledgeDto dto)
        {
            var result = ManagementContract.KnowledgeInsert(dto);
            return DeyiJson(result, true);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("question-count")]
        public int QuestionCount(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return 0;
            return ManagementContract.KnowledgeQuestionCount(code);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("move")]
        public ActionResult MovePoint(string source, string target)
        {
            return DeyiJson(ManagementContract.KnowledgeMove(source, target), true);
        }

        [AjaxOnly]
        [HttpGet]
        [Route("list")]
        public ActionResult List(int stage, int subjectId, string keyword)
        {
            var result = ManagementContract.KnowledgeSearch(new KnowledgeSearchDto
            {
                Stage = stage,
                SubjectId = subjectId,
                Keyword = keyword
            });
            if (!result.Status || result.Data == null)
                return DeyiJson(result);
            return DeyiJson(result.Data.Select(t => new
            {
                path = ManagementContract.KnowledgePath(t.PID),
                t.Code,
                t.Name
            }));
        }

        [AjaxOnly]
        [HttpPost]
        [Route("reset-mover")]
        public ActionResult ResetMover(string code)
        {
            return DeyiJson(KnowledgeMover.Instance.ResetMover(code));
        }

        [AjaxOnly]
        [HttpPost]
        [Route("cancel-mover")]
        public ActionResult CancelMover(string code)
        {
            return DeyiJson(KnowledgeMover.Instance.CancelMover(code));
        }

        #endregion
    }
}