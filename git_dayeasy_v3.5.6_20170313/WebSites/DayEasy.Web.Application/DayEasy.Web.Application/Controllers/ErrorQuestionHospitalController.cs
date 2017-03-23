using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Contracts.Enum;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Web.Filters;
using System.Web.Mvc;

namespace DayEasy.Web.Application.Controllers
{
    /// <summary> 错题医院 </summary>
    [RoleAuthorize(UserRole.Teacher, "/")]
    [RoutePrefix("hospital")]
    public class ErrorQuestionHospitalController : DController
    {
        private readonly IGroupContract _groupContract;
        private readonly ISystemContract _systemContract;
        private readonly IErrorBookContract _errorBookContract;
        public ErrorQuestionHospitalController(IUserContract userContract,
            IGroupContract groupContract, ISystemContract systemContract,
            IErrorBookContract errorBookContract
            ) : base(userContract)
        {
            _groupContract = groupContract;
            _systemContract = systemContract;
            _errorBookContract = errorBookContract;
        }
        /// <summary>
        /// 错题列表
        /// </summary>
        /// <returns></returns>
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }
        [Route("student-view")]
        public ActionResult StudentView(string groupId)
        {
            return View();
        }
        /// <summary>
        /// 错题学生列表
        /// </summary>
        /// <returns></returns>
        [Route("errorUserList")]
        public ActionResult ErrorUserList()
        {
            return View();
        }
        /// <summary>
        /// 错误信息学生列表
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [Route("errorUsers")]
        public ActionResult ErrorUsers(string groupId)
        {
            return DJson.Json(_errorBookContract.ErrorUsers(groupId, CurrentUser.SubjectId));
        }
        /// <summary>
        /// 问题类型
        /// </summary>
        /// <returns></returns>
        [Route("questionTypes")]
        [HttpGet]
        public ActionResult QuestionTypes()
        {
            var types = SystemCache.Instance.SubjectQuestionTypes(CurrentUser.SubjectId);
            return DJson.Json(types);
        }
        /// <summary>
        /// 获取圈子信息
        /// </summary>
        /// <returns></returns>
        [Route("groups")]
        public ActionResult Groups()
        {
            var groups = _groupContract.Groups(UserId, (byte)GroupType.Class);
            if (!groups.Status)
                return DJson.Json(DResult.Error("没有获取到圈子信息"));
            return DJson.Json(groups.Data);
        }
        /// <summary>
        /// 知识点
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Route("knowledges")]
        public ActionResult Knowledges(SearchErrorQuestionDto dto)
        {
            dto.SubjectId = CurrentUser.SubjectId;
            return DJson.Json(_errorBookContract.Knowledges(dto));
        }
        /// <summary>
        /// 获取题目信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Route("errorQuestions")]
        public ActionResult ErrorQuestions(SearchErrorQuestionDto dto)
        {
            dto.SubjectId = CurrentUser.SubjectId;
            return DJson.Json(_errorBookContract.ErrorQuestions(dto));
        }
    }
}