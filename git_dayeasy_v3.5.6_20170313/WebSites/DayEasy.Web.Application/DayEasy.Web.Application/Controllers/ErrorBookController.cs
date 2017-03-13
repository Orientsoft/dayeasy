using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    [RoleAuthorize(UserRole.Student, "/")]
    [RoutePrefix("errorbook")]
    public class ErrorBookController : DController
    {
        private readonly IErrorBookContract _errorBookContract;
        private readonly IPublishContract _publishContract;
        private readonly IGroupContract _groupContract;

        public ErrorBookController(IUserContract userContract,
            IErrorBookContract errorBookContract,
            IPublishContract publishContract,
            IGroupContract groupContract)
            : base(userContract)
        {
            _errorBookContract = errorBookContract;
            _publishContract = publishContract;
            _groupContract = groupContract;
            ViewBag.PageNav = 2;
        }

        /// <summary> 列表 </summary>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Questions(
            int index, int size, int subject, int type,
            string start, string expire, string key, int reason = -1, int pass = -1)
        {
            var def = DateTime.Parse("1900-01-01");
            var dts = ConvertHelper.StrToDateTime(start, def);
            var dte = ConvertHelper.StrToDateTime(expire, def);

            var search = new ErrorQuestionSearchDto
            {
                Page = DPage.NewPage(index - 1, size),
                StudentId = ChildOrUserId,
                SubjectId = subject,
                QType = type,
                Key = key,
                StartTime = null,
                EndTime = null,
                HasReason = null
            };
            if (pass >= 0)
                search.IsPass = (pass == 1);
            if (reason >= 0)
                search.HasReason = reason == 1;
            if (dts != def)
                search.StartTime = dts;
            if (dte != def)
                search.EndTime = dte.AddDays(1);

            return DJson.Json(_errorBookContract.ErrorQuestions(search),
                namingType: NamingType.UrlCase);
        }

        public ActionResult Subjects()
        {
            return DJson.Json(_errorBookContract.ErrorQuestionSubjects(ChildOrUserId).ToArray(),
                namingType: NamingType.UrlCase);
        }

        [Route("question-type")]
        public ActionResult QuestionType(int subjectId)
        {
            var list = _errorBookContract.ErrorQuestionTypes(ChildOrUserId, subjectId);
            if (list == null || !list.Any())
                return DJson.Json(DResult.Error("没有查询到相关题型"));
            return DJson.Json(DResult.Succ(list, list.Count), namingType: NamingType.UrlCase);
        }

        [Route("search-keys")]
        public ActionResult SearchKeys(string key, int subjectId)
        {
            return DJson.Json(_errorBookContract.SearchKeys(key, subjectId),
                namingType: NamingType.UrlCase);
        }

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(string id)
        {
            if (id.IsNullOrEmpty())
                return View();
            ViewData["currentUser"] = CurrentUser;

            var qr = _errorBookContract.ErrorQuestion(id, ChildOrUserId);
            if (!qr.Status) return View();
            var item = qr.Data;
            var groupResult = _groupContract.LoadById(item.GroupId);
            ViewData["groupId"] = item.GroupId;
            ViewData["groupName"] = groupResult.Status && groupResult.Data != null
                ? groupResult.Data.Name
                : string.Empty;

            //变式题
            if (item.SourceType == (byte)ErrorQuestionSourceType.Paper)
            {
                var qvs = _publishContract.Variant(item.Question.Id);
                if (qvs.Status)
                {
                    var qv = qvs.Data.FirstOrDefault();
                    if (qv != null)
                        ViewData["Variant"] = qv;
                }
            }

            //查询班级内所有同学的错因分析
            var reasonResult = _errorBookContract.Reasons(id, item.Batch, item.Question.Id);
            if (reasonResult.Status)
                ViewData["reasons"] = reasonResult.Data;

            return View(item);
        }

        /// <summary> 过关、取消过关 </summary>
        public ActionResult Pass(string id, bool isPass)
        {
            return DJson.Json(_errorBookContract.SetPass(id, ChildOrUserId, isPass));
        }

        /// <summary> 学生错题答案 </summary>
        /// <param name="batch"></param>
        /// <param name="paper_id"></param>
        /// <param name="question_id"></param>
        /// <returns></returns>
        public ActionResult Answer(string batch, string paper_id, string question_id)
        {
            return DJson.Json(_errorBookContract.ErrorAnswer(
                batch, paper_id, question_id, ChildOrUserId), namingType: NamingType.UrlCase);
        }

    }
}