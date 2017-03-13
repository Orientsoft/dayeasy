using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 变式 </summary>
    [RoleAuthorize(UserRole.Teacher, "/")]
    public class VariantController : DController
    {
        private readonly IPaperContract _paperContract;
        private readonly IPublishContract _publishContract;
        public VariantController(IUserContract userContract, IPaperContract paperContract, IPublishContract publishContract)
            : base(userContract)
        {
            _paperContract = paperContract;
            _publishContract = publishContract;
            ViewBag.PageNav = 2;
        }

        /// <summary> 变式推送 </summary>
        /// <param name="paper_id">试卷ID</param>
        /// <param name="question_id">问题ID</param>
        /// <returns></returns>
        public ActionResult Index(string paper_id, string question_id)
        {
            if (paper_id.IsNullOrEmpty() || question_id.IsNullOrEmpty()) return View();

            var question = _paperContract.LoadQuestion(question_id);
            if (question == null) return View();

            List<QuestionDto>
                sysVariant = new List<QuestionDto>(),
                teacherVariant = new List<QuestionDto>();

            //历史推荐
            var historyResult = _publishContract.VariantHistory(paper_id, question_id, CurrentUser.Id);
            if (historyResult.Status && historyResult.Data.Any())
            {
                return View(new ViewDataDictionary
                {
                    {"questionId", question_id},
                    {"paperId", paper_id},
                    {"question", question},
                    {"historys", historyResult.Data.ToList()}
                });
            }

            //系统推荐
            var sysResult = _publishContract.Variant(question_id, 4);
            if (sysResult.Status) sysVariant = sysResult.Data.ToList();

            //教师推荐 - 变式关系表
            var relationResult = _publishContract.VariantRelationQuestion(question_id, 4);
            if (relationResult.Status)
                teacherVariant = relationResult.Data.ToList();

            return View(new ViewDataDictionary
            {
                {"questionId", question_id},
                {"paperId", paper_id},
                {"question", question},
                {"sysVariant", sysVariant},
                {"teacherVariant", teacherVariant}
            });
        }

        public ActionResult Add(string paper_id, string question_id, string vid)
        {
            List<string> vids = null;
            if (vid.IsNotNullOrEmpty()) vids = JsonHelper.JsonList<string>(vid).ToList();
            return DJson.Json(_publishContract.AddVariant(CurrentUser.Id, paper_id, question_id, vids),
                namingType: NamingType.UrlCase);
        }

    }
}