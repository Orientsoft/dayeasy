using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Services.Helper;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 题库 </summary>
    [RoleAuthorize(UserRole.Teacher, "/")]
    [RoutePrefix("question")]
    public class QuestionController : DController
    {
        private readonly IPaperContract _paperContract;
        private const string AppName = "题库";

        public QuestionController(IUserContract userContract, IPaperContract paperContract)
            : base(userContract)
        {
            _paperContract = paperContract;
            ViewBag.AppName = AppName;
        }

        #region 视图Action
        /// <summary> 题库列表 </summary>
        [Route("")]
        public ActionResult Index()
        {
            ViewData["stages"] = StageList;
            return View();
        }

        /// <summary> 添加题目 </summary>
        [Route("add")]
        public ActionResult Add()
        {
            return View();
        }

        /// <summary> 修改题目 </summary>
        [Route("modify/{id:regex([0-9a-z]{32})}")]
        public ActionResult Modify(string id)
        {
            return View();
        }

        [Route("edit-body/{id:regex([0-9a-z]{32})}")]
        public ActionResult EditBody(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return MessageView("试题ID不能为空！");
            var question = _paperContract.LoadQuestion(id);
            if (question == null)
                return MessageView("试题不存在！");
            //            if(question.UserId != UserId)
            //                return MessageView("不能编辑其他人的试题！");
            ViewData["id"] = id;
            ViewData["body"] = question.Body;
            return PartialView();
        }

        #endregion

        #region Ajax操作

        [HttpPost]
        [Route("init")]
        public ActionResult Init()
        {
            if (CurrentUser.SubjectId <= 0)
                return Json(new { error = "没有所属科目！" });
            if (!CurrentUserRoles.Contains(UserRole.Teacher))
                return Json(new { error = "不是教师帐号！" });
            var stages = StageList.Select(t => new
            {
                id = t.Key,
                name = t.Value
            });
            return DeyiJson(new
            {
                subject_id = CurrentUser.SubjectId,
                stages,
                types = SystemCache.Instance.SubjectQuestionTypes(CurrentUser.SubjectId)
            }, true);
        }

        /// <summary> 搜索题目 </summary>
        [HttpPost]
        [Route("search")]
        public ActionResult Search(byte range, int type, string keyword, int page, int size, int stage)
        {
            var pageInfo = DPage.NewPage(page, size);
            var query = new SearchQuestionDto
            {
                UserId = CurrentUser.Id,
                QuestionType = type,
                Stages = stage > 0 ? new[] { stage } : StageList.Select(t => t.Key).ToArray(),
                ShareRange = range,
                Keyword = keyword,
                SubjectId = CurrentUser.SubjectId,
                IsHighLight = !string.IsNullOrWhiteSpace(keyword),
                Page = pageInfo.Page,
                Size = pageInfo.Size,
                LoadCreator = true
            };
            if (string.IsNullOrWhiteSpace(keyword))
            {
                query.Order = QuestionOrder.AddedAtDesc;
            }
            return DeyiJson(_paperContract.SearchQuestion(query), true);
        }

        /// <summary> 删除题目 </summary>
        [HttpPost]
        [Route("remove")]
        public ActionResult Remove(string id)
        {
            var result = _paperContract.DeleteQuestion(id, CurrentUser.Id);
            return DeyiJson(result, true);
        }

        /// <summary> 加载题目 </summary>
        [Route("load/{id:regex([0-9a-z]{32})}")]
        public ActionResult Load(string id)
        {
            var item = _paperContract.LoadQuestion(id, false);
            if (item == null)
                return DJson.Json(DResult.Error("问题未找到！"));
            return DeyiJson(DResult.Succ(item));
        }

        /// <summary> 保存 </summary>
        [HttpPost]
        [Route("save")]
        public ActionResult Save()
        {
            var vqQuestion = FromBody<QuestionDto>();
            if (vqQuestion == null)
                return DJson.Json(DResult.Error("提交数据异常，请修改后重试！"));
            vqQuestion.UserId = CurrentUser.Id;
            vqQuestion.SubjectId = CurrentUser.SubjectId;
            vqQuestion.HasSmall = (vqQuestion.Details != null && vqQuestion.Details.Any());
            var result = _paperContract.SaveQuestion(vqQuestion);
            return DeyiJson(result, true);
        }

        /// <summary> 另存为 </summary>
        [HttpPost]
        [Route("save-as")]
        public ActionResult SaveAs()
        {
            var vqQuestion = FromBody<QuestionDto>();
            if (vqQuestion == null)
                return DJson.Json(DResult.Error("提交数据异常，请修改后重试！"));
            if (vqQuestion.UserId > 0 && vqQuestion.UserId != CurrentUser.Id)
                return DJson.Json(DResult.Error("没有修改问题的权限！"));
            vqQuestion.UserId = CurrentUser.Id;
            var result = _paperContract.SaveQuestion(vqQuestion, true);
            return DeyiJson(result, true);
        }

        [HttpPost]
        [Route("save-knowledge")]
        public ActionResult SaveKnowledges(string id, string knowledges)
        {
            knowledges = (knowledges ?? string.Empty).UrlDecode();
            var list = JsonHelper.JsonList<NameDto>(knowledges).ToList();
            return DeyiJson(_paperContract.SaveQuestionKnowledge(id, list), true);
        }

        [HttpPost]
        [Route("save-body")]
        public ActionResult SaveBody(string id, string body)
        {
            body = body.UrlDecode();
            return DeyiJson(_paperContract.SaveQuestionBody(id, body));
        }

        [HttpPost]
        [Route("range")]
        public ActionResult UpdateRange(string id, byte range)
        {
            var result = _paperContract.QuestionShare(id, (ShareRange)range, UserId);
            return DeyiJson(result, true);
        }
        #endregion
    }
}