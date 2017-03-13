using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Services.Helper;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 系统异步方法 </summary>
    [RoleAuthorize(UserRole.Teacher, "/")]
    [RoutePrefix("sys")]
    public class SystemController : DController
    {
        private readonly ISystemContract _systemContract;

        public SystemController(IUserContract userContract, ISystemContract systemContract)
            : base(userContract)
        {
            _systemContract = systemContract;
        }

        /// <summary> 获取题型 </summary>
        /// <param name="subject_id">科目ID</param>
        [Route("qtypes")]
        public ActionResult QuestionTypes(int subject_id = 0)
        {
            if (subject_id == 0 && CurrentUser != null)
            {
                subject_id = CurrentUser.SubjectId;
            }
            return DJson.Json(SystemCache.Instance.SubjectQuestionTypes(subject_id),
                namingType: NamingType.CamelCase);
        }

        /// <summary> 获取关联题型 </summary>
        /// <param name="type">题型Id</param>
        [Route("qtype/{type:int}")]
        public ActionResult QuestionType(int type)
        {
            return DJson.Json(_systemContract.GetQuestionTypes(type), namingType: NamingType.CamelCase);
        }

        [HttpPost]
        [Route("knowledges")]
        public ActionResult Knowledges(int subject = -1, int stage = 0, int parentId = -1, string keyword = null)
        {
            if (stage <= 0)
            {
                return DJson.Json(DResult.Error("请选择学段！"));
            }
            var knowledges = _systemContract.Knowledges(new SearchKnowledgeDto
            {
                ParentId = parentId,
                Stage = (byte)stage,
                SubjectId = subject > 0 ? subject : CurrentUser.SubjectId,
                Keyword = (keyword ?? string.Empty).Trim(),
                Page = 0,
                Size = 15,
                LoadPath = true
            }).Select(t => new
            {
                id = t.Code,
                name = t.Name,
                path =
                    (t.Parents.Count > 0
                        ? t.Parents.Values.Aggregate(string.Empty, (c, v) => c + v + ">") + t.Name
                        : t.Name)
            });
            return DJson.Json(knowledges);
        }

        /// <summary> 获取知识点树 </summary>
        /// <param name="stage"></param>
        /// <param name="subject_id"></param>
        /// <param name="parent_id"></param>
        /// <returns></returns>
        [Route("kpoints")]
        public ActionResult Kpoints(byte stage, int subject_id = -1, int parent_id = 0)
        {
            return DJson.Json(_systemContract.KnowledgeTrees(new SearchKnowledgeDto
            {
                ParentId = parent_id,
                Stage = stage,
                SubjectId = subject_id,
                Page = 0,
                Size = 200
            }).Data);
        }
    }
}