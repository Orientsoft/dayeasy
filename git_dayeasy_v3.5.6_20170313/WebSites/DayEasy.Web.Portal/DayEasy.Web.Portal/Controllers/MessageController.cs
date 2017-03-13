using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Utility.Extend;
using DayEasy.Web.Filters;

namespace DayEasy.Web.Portal.Controllers
{
    [DAuthorize(true, false)]
    [RoutePrefix("message")]
    public class MessageController : DController
    {
        private readonly IMessageContract _messageContract;
        private readonly ICommentContract _commentContract;

        public MessageController(IUserContract userContract, IMessageContract messageContract,
            ICommentContract commentContract)
            : base(userContract)
        {
            _messageContract = messageContract;
            _commentContract = commentContract;
        }

        /// <summary> 评论 </summary>
        /// <param name="sourceId"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [Route("comment-list")]
        public ActionResult CommentList(string sourceId, int pageindex = 1, int pagesize = 300)
        {
            var result = _commentContract.CommentList(new CommentSearchDto
            {
                SourceId = sourceId,
                Page = pageindex - 1,
                Size = pagesize
            });
            ViewBag.SourceId = sourceId;
            return PartialView("Helper/CommentHelper", result.Data);
        }

        /// <summary> 评论 </summary>
        /// <param name="sourceId"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("comments-data")]
        public ActionResult CommentListData(string sourceId, int index = 0, int size = 300)
        {
            var result = _commentContract.CommentList(new CommentSearchDto
            {
                SourceId = sourceId,
                Page = index,
                Size = size
            });
            return new JsonpResult(result.Data, "callback".Query(string.Empty));
        }

        [Route("count")]
        public ActionResult MessageCount()
        {
            return new JsonpResult(_messageContract.UserMessageCount(UserId), "callback".Query(string.Empty));
        }

        /// <summary> 获取圈子动态 </summary>
        /// <param name="groupId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [AjaxOnly]
        [HttpPost]
        [Route("dynamics")]
        public ActionResult GroupDynamics(string groupId, int page = 0, int size = 15)
        {
            var dto = new DynamicSearchDto
            {
                UserId = ChildOrUserId,
                GroupId = groupId,
                Role = (CurrentUser.IsTeacher() ? UserRole.Teacher : UserRole.Student),
                Page = page,
                Size = size
            };
            if (Children != null && Children.Any())
                dto.ParentId = UserId;
            var dynamics = _messageContract.GetDynamics(dto);
            return DeyiJson(dynamics);
        }

        /// <summary> 发通知 </summary>
        /// <param name="groupId"></param>
        /// <param name="message"></param>
        /// <param name="classes"></param>
        /// <returns></returns>
        [AjaxOnly]
        [HttpPost]
        [Route("send_notice")]
        [RoleAuthorize(UserRole.Teacher, "/")]
        public ActionResult SendNotice(string groupId, string message, string classes = null)
        {
            var list = new List<string> { groupId };
            if (!string.IsNullOrWhiteSpace(classes))
            {
                list.AddRange(classes.Split(','));
            }

            var result = _messageContract.SendDynamics(list.Select(t => new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Notice,
                GroupId = t,
                ReceivRole = UserRole.Student | UserRole.Teacher,
                UserId = UserId,
                Message = message
            }));
            return DeyiJson(result);
        }

        /// <summary> 动态点赞 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxOnly]
        [HttpPost]
        [Route("support")]
        public ActionResult SupportDynamic(string id)
        {
            var result = _messageContract.LikeDynamic(id, UserId);
            return DeyiJson(result);
        }

        /// <summary> 发表评论 </summary>
        [AjaxOnly]
        [HttpPost]
        [Route("comment")]
        public ActionResult Comment(string sourceId, string message, string parentId = null)
        {
            var result = _commentContract.Comment(sourceId, UserId, message, parentId);
            if (result.Status)
            {
                if (sourceId.StartsWith("topic_"))//帖子
                {
                    sourceId = sourceId.Replace("topic_", "").Trim();
                    CurrentIocManager.Resolve<ITopicContract>().UpdateTopicReplyNum(sourceId);
                }
                else
                {
                    _messageContract.UpdateDynamicCommentCount(sourceId, 1);
                }
            }
            return DeyiJson(result);
        }

        [AjaxOnly]
        [HttpPost]
        [Route("delete-comment")]
        public ActionResult DeleteComment(string sourceId, string id)
        {
            var result = _commentContract.Delete(id, UserId);
            if (result.Status)
            {
                if (sourceId.StartsWith("topic_"))//帖子
                {
                    sourceId = sourceId.Replace("topic_", "").Trim();
                    CurrentIocManager.Resolve<ITopicContract>().UpdateTopicReplyNum(sourceId, true);
                }
                else
                {
                    _messageContract.UpdateDynamicCommentCount(sourceId, 0 - result.Data);
                }
            }
            return DeyiJson(result);
        }
    }
}