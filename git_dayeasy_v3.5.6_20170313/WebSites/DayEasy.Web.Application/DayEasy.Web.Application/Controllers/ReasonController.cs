using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Core.Domain;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Application.Controllers
{
    [DAuthorize]
    public class ReasonController : DController
    {
        private readonly IErrorBookContract _errorBookContract;
        private readonly IAnswerShareContract _answerShareContract;

        public ReasonController(IUserContract userContract,
            IErrorBookContract errorBookContract,
            IAnswerShareContract answerShareContract) : base(userContract)
        {
            _errorBookContract = errorBookContract;
            _answerShareContract = answerShareContract;
            ViewBag.PageNav = 2;
        }

        #region 错因标签、评论、错误率
        
        /// <summary>
        /// 初始化错因分析
        /// </summary>
        /// <returns></returns>
        public ActionResult Load(string id,string batch,string question_id)
        {
            if (id.IsNullOrEmpty())
                id = _errorBookContract.GetErrorId(batch, question_id, ChildOrUserId);
            return DJson.Json(_errorBookContract.Load(id), namingType: NamingType.UrlCase);
        }

        /// <summary>
        /// 查询班级和年级的错误率
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="paper_id"></param>
        /// <param name="question_id"></param>
        /// <returns></returns>
        public ActionResult Rates(string batch, string paper_id, string question_id)
        {
            var json = _errorBookContract.ErrorQuestionRate(batch, paper_id, question_id);
            return DJson.Json(json, namingType: NamingType.UrlCase);
        }

        /// <summary>
        /// 添加错因
        /// </summary>
        /// <returns></returns>
        public ActionResult Add(string id, string content, string tag)
        {
            List<string> tagList = null;
            if (tag.IsNotNullOrEmpty())
            {
                tag = HttpUtility.UrlDecode(tag);
                tagList = tag.JsonToObject<List<string>>();
                tagList.ForEach(t => t = HttpUtility.UrlDecode(t));
            }
            if (content.IsNotNullOrEmpty())
                content = HttpUtility.UrlDecode(content);
            return DJson.Json(_errorBookContract.AddReason(id, content, tagList));
        }

        /// <summary>
        /// 删除错因分析及评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(string id)
        {
            return DJson.Json(_errorBookContract.DeleteComment(id, ChildOrUserId));
        }

        /// <summary>
        /// 推荐标签
        /// </summary>
        /// <param name="id">错题ID</param>
        /// <returns></returns>
        public ActionResult RecomTags(string id)
        {
            return DJson.Json(_errorBookContract.RecomTags(id));
        }

        /// <summary>
        /// 评论列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public ActionResult Comments(string id, string pid, int index)
        {
            var page = DPage.NewPage((index - 1), 10);
            return DJson.Json(_errorBookContract.Comments(page, id, ""), namingType: NamingType.UrlCase);
        }

        /// <summary>
        /// 添加评论
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <param name="uid"></param>
        /// <param name="uname"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public ActionResult AddComment(string id, string content, string pid, long uid, string uname)
        {
            var json = _errorBookContract.AddComment(id, pid, content, ChildOrUserId, uid, uname);
            return DJson.Json(json);
        }

        #endregion

        #region 答案分享、膜拜

        /// <summary>
        /// 同学分享的答案
        /// </summary>
        /// <param name="question_id"></param>
        /// <param name="group_id"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public ActionResult Shares(string question_id, string group_id, bool all)
        {
            return DJson.Json(_answerShareContract.Shares(question_id, group_id, all),
                namingType: NamingType.UrlCase);
        }

        /// <summary>
        /// 分享答案详细
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(string id)
        {
            return DJson.Json(_answerShareContract.Detail(id, ChildOrUserId),
                namingType: NamingType.UrlCase);
        }


        /// <summary>
        /// 膜拜
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Worship(string id)
        {
            return DJson.Json(_answerShareContract.Worship(id, CurrentUser.Id, CurrentUser.Name),
                namingType: NamingType.UrlCase);
        }

        #endregion

    }
}