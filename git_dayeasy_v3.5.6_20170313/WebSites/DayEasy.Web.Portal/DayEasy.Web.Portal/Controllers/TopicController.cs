using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Topic;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Portal.Controllers
{
    /// <summary> 帖子 </summary>
    [RoutePrefix("topic")]
    public class TopicController : DController
    {
        private readonly IGroupContract _groupContract;
        private readonly ITopicContract _topicContract;
        private readonly ICommentContract _commentContract;

        public TopicController(IUserContract userContract, IGroupContract groupContract, ITopicContract topicContract, ICommentContract commentContract)
            : base(userContract)
        {
            _groupContract = groupContract;
            _topicContract = topicContract;
            _commentContract = commentContract;
        }

        #region 频道主页
        /// <summary>
        /// 频道主页
        /// </summary>
        /// <returns></returns>
        [Route("{id:int}/{pageindex?}")]
        public ActionResult Index(int id, int pageindex = 1)
        {
            return Redirect(Consts.Config.MainSite);
//            if (id < 1)
//                return Redirect(Consts.Config.MainSite);
//            ViewBag.PageNav = 3;
//
//            var channels = Consts.GetChannels(id);//查询频道
//            if (channels == null || !channels.Exists(u => u.Id == id))
//                return Redirect(Consts.Config.MainSite);
//
//            ViewData["Channels"] = channels;
//
//            var pageIndex = pageindex;
//            int totalCount = 0;
//
//            var type = "t".Query("");
//            if (type == "group")//查询圈子
//            {
//                var newGroups = _groupContract.SearchShareGroups(id, ShareGroupOrder.AddAtDesc,
//                    DPage.NewPage(pageIndex - 1, 14));
//
//                ViewData["newGroups"] = newGroups.Status ? newGroups.Data.ToList() : null;
//                totalCount = newGroups.Status ? newGroups.TotalCount : 0;
//            }
//            else//查询最新帖子
//            {
//                var newTopics = _topicContract.GetTopics(new SearchTopicDto()
//                {
//                    ClassType = id,
//                    Page = pageIndex - 1,
//                    Size = 14,
//                    Order = TopicOrder.TimeDesc
//                });
//                ViewData["newTopics"] = newTopics.Status ? newTopics.Data.ToList() : null;
//                totalCount = newTopics.Status ? newTopics.TotalCount : 0;
//            }
//
//            ViewData["totalCount"] = totalCount;
//            ViewData["pageIndex"] = pageIndex;
//
//            //查询热帖
//            var hotTopics = _topicContract.GetTopics(new SearchTopicDto()
//            {
//                ClassType = id,
//                Page = 0,
//                Size = 6,
//                Order = TopicOrder.ReadNum
//            });
//            ViewData["hotTopics"] = hotTopics.Status ? hotTopics.Data.ToList() : null;
//
//            //查询热圈子
//            var hotGroups = _groupContract.SearchShareGroups(id, ShareGroupOrder.TopicNumDesc,
//                    DPage.NewPage(0, 6));
//            ViewData["hotGroups"] = hotGroups.Status ? hotGroups.Data.ToList() : null;
//
//            return View(channels.SingleOrDefault(t => t.Id == id));
        }
        #endregion

        #region 帖子详情
        /// <summary>
        /// 帖子详情
        /// </summary>
        /// <returns></returns>
        [Route("detail/{id:length(32)}")]
        public ActionResult TopicDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return View();
            ViewBag.PageNav = 3;

            var topic = _topicContract.GetTopicDetail(id, CurrentUser == null ? 0 : CurrentUser.Id);
            if (topic == null) return View();

            //后台帖子管理链接过来的
            var from = "from".Query("");
            if (!string.IsNullOrEmpty(from) && from.Trim() == "f494f34bdf594bad8af5336a3d38812b")
                return View(topic);

            //圈子
            var group = _groupContract.LoadById(topic.GroupId);
            if (!group.Status || group.Data == null) return View();

            var shareGroup = group.Data as ShareGroupDto;
            if (shareGroup == null)
                return View();

            if (shareGroup.JoinAuth == (byte)GroupJoinAuth.Private && CurrentUser == null)
                return View();

            if (CurrentUser != null)
            {
                var checkStatus = _groupContract.IsGroupMember(CurrentUser.Id, group.Data.Id);

                if (shareGroup.JoinAuth == (byte)GroupJoinAuth.Private && checkStatus != CheckStatus.Normal)
                    return View();

                if (checkStatus == CheckStatus.Normal)//验证是否已经在圈子
                    ViewData["isMember"] = true;

                if (shareGroup.ManagerId == CurrentUser.Id)
                    ViewData["isManager"] = true;

                if (topic.AddedBy == CurrentUser.Id)
                    ViewData["isMine"] = true;
            }

            if (shareGroup.Status != (byte)NormalStatus.Delete)
                ViewData["group"] = shareGroup;

            //相关文章
            var otherTopics = _topicContract.GetTopics(new SearchTopicDto()
            {
                ClassType = topic.ClassType,
                Tags = topic.TagList,
                Page = 0,
                Size = 6,
                Order = TopicOrder.ReadNum,
                ExceptIds = new List<string>() { topic.Id }
            });
            ViewData["otherTopics"] = otherTopics.Status ? otherTopics.Data.ToList() : null;

            //更新阅读数量
            _topicContract.UpdateTopicReadNum(topic.Id);

            return View(topic);
        }
        #endregion

        #region 评论
        /// <summary> 评论 </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        [Route("comment-list")]
        public ActionResult CommentList(string sourceId)
        {
            var result = _commentContract.CommentList(new CommentSearchDto
            {
                SourceId = sourceId,
                Page = 0,
                Size = 15
            });
            ViewBag.SourceId = sourceId;
            return PartialView("Helper/CommentHelper", result.Data);
        }
        #endregion

        #region 发帖子

        #region 发帖子--显示
        /// <summary>
        /// 发帖子--显示
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [Route("pub/{id}")]
        public ActionResult PubTopic(string id)
        {
            if (string.IsNullOrEmpty(id))
                return MessageView("没有发帖权限！");

            var group = _groupContract.LoadById(id);
            if (!group.Status || group.Data == null)
                return MessageView("没有发帖权限！");

            var shareGroup = group.Data as ShareGroupDto;
            if (shareGroup == null || shareGroup.Type != (byte)GroupType.Share)
                return MessageView("没有发帖权限！");

            if (shareGroup.ManagerId == CurrentUser.Id) return View(group.Data);

            if (shareGroup.PostAuth == (byte)GroupPostAuth.Owner)
                return MessageView("没有发帖权限！");

            var result = _groupContract.IsGroupMember(CurrentUser.Id, shareGroup.Id);
            if (result != CheckStatus.Normal)
                return MessageView("没有发帖权限！");

            return View(group.Data);
        }
        #endregion

        #region 发帖子--保存
        /// <summary>
        /// 发帖子--保存
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [Route("save")]
        [HttpPost]
        public ActionResult SaveTopic()
        {
            var topic = "topic".Form("");
            if (string.IsNullOrEmpty(topic))
                return Json(DResult.Error("参数错误，请刷新页面重试！"), JsonRequestBehavior.AllowGet);

            var topicData = topic.JsonToObject<PubTopicDto>();
            if (topicData == null)
                return Json(DResult.Error("参数错误，请刷新页面重试！"), JsonRequestBehavior.AllowGet);

            topicData.UserId = CurrentUser.Id;
            var result = _topicContract.PublishTopic(topicData);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region 编辑帖子

        #region 编辑帖子--显示
        /// <summary>
        /// 编辑帖子--显示
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [Route("edit/{id:length(32)}")]
        public ActionResult EditTopic(string id)
        {
            if (string.IsNullOrEmpty(id))
                return MessageView("参数错误！");

            var topic = _topicContract.GetTopicDetail(id, CurrentUser.Id);
            if (topic == null)
                return MessageView("没有找到该帖子！");

            if (topic.AddedBy != CurrentUser.Id)
                return MessageView("您没有修改该帖子的权限！");

            return View(topic);
        }
        #endregion

        #region 编辑帖子--保存
        /// <summary>
        /// 编辑帖子--保存
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [HttpPost]
        [Route("edit/save/{id:length(32)}")]
        public ActionResult EditSave(string id)
        {
            var topic = "topic".Form("");

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(topic))
                return Json(DResult.Error("参数错误，请刷新页面重试！"), JsonRequestBehavior.AllowGet);

            var topicData = topic.JsonToObject<PubTopicDto>();
            if (topicData == null)
                return Json(DResult.Error("参数错误，请刷新页面重试！"), JsonRequestBehavior.AllowGet);

            topicData.UserId = CurrentUser.Id;
            var result = _topicContract.EditTopic(id, topicData);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region 删除帖子
        /// <summary>
        /// 删除帖子
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [HttpPost]
        [Route("delete/{id:length(32)}")]
        public ActionResult TopicDelete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(DResult.Error("参数错误！"), JsonRequestBehavior.AllowGet);

            var result = _topicContract.DeleteTopic(id, CurrentUser.Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除帖子的投票
        /// <summary>
        /// 删除帖子的投票
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [HttpPost]
        [Route("deletevote/{id:length(32)}")]
        public ActionResult DeleteVote(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(DResult.Error("参数错误！"), JsonRequestBehavior.AllowGet);

            var result = _topicContract.DeleteVote(id, CurrentUser.Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 加精 or 取消加精 or 置顶 or 取消置顶
        /// <summary>
        /// 加精 or 取消加精 or 置顶 or 取消置顶
        /// </summary>
        /// <returns></returns>
        [DAuthorize]
        [HttpPost]
        [Route("updatestate/{id:length(32)}")]
        public ActionResult TopicUpdateState(string id)
        {
            var state = "state".Form(-1);

            if (string.IsNullOrEmpty(id) || !Enum.IsDefined(typeof(TopicState), (byte)state))
                return Json(DResult.Error("参数错误！"), JsonRequestBehavior.AllowGet);

            var result = _topicContract.UpdateTopicState(id, CurrentUser.Id, (TopicState)state);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 点赞帖子
        /// <summary>
        /// 点赞帖子
        /// </summary>
        /// <returns></returns>
        [DAuthorize(true, false)]
        [HttpPost]
        [Route("praise/{id:length(32)}")]
        public ActionResult PraiseTopic(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(DResult.Error("参数错误！"), JsonRequestBehavior.AllowGet);

            var result = _topicContract.UpdateTopicPraise(id, CurrentUser.Id, true);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 投票
        /// <summary>
        /// 投票
        /// </summary>
        /// <returns></returns>
        [DAuthorize(true, false)]
        [HttpPost]
        [Route("castvote/{id:length(32)}")]
        public ActionResult CastVote(string id)
        {
            var options = "options".Form("");

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(options))
                return Json(DResult.Error("参数错误！"), JsonRequestBehavior.AllowGet);

            var optionList = options.JsonToObject<List<string>>();

            var result = _topicContract.CastVote(id, CurrentUser.Id, optionList);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Ajax

        [AjaxOnly]
        [HttpGet]
        [Route("search")]
        public ActionResult TopicList(string groupId, bool isPick = false, int page = 0, int size = 15)
        {
            var result = _topicContract.GetTopics(new SearchTopicDto
            {
                GroupId = groupId,
                State = (isPick ? (byte)TopicState.Pick : (byte)TopicState.Normal),
                Page = page,
                Size = size
            });
            return DeyiJson(result);
        }

        #endregion
    }
}