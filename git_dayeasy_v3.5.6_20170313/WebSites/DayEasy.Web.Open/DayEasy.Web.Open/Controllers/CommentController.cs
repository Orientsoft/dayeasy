using System.Web.Http;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Core.Dependency;
using DayEasy.Models.Open.Comment;
using DayEasy.Utility;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;

namespace DayEasy.Web.Open.Controllers
{
    /// <summary> 评论回复通用 </summary>
    [DApi]
    public class CommentController : DApiController
    {
        private readonly ICommentContract _commentContract;
        public CommentController(IUserContract userContract, ICommentContract commentContract)
            : base(userContract)
        {
            _commentContract = commentContract;
        }

        /// <summary> 评论列表 </summary>
        [HttpGet]
        public DResult<CommentResultDto> List([FromUri] MCommentSearchInputDto searchDto)
        {
            return _commentContract.CommentList(new CommentSearchDto
            {
                SourceId = searchDto.SourceId,
                Page = searchDto.Page,
                Size = searchDto.Size
            });
        }

        /// <summary> 回复评论 </summary>
        [HttpPost]
        [DApiAuthorize]
        public DResult<CommentDto> Send(MCommentSendInputDto dto)
        {
            if (UserId <= 0)
            {
                return DResult.Error<CommentDto>("请先登录！");
            }
            var result = _commentContract.Comment(dto.SourceId, UserId, dto.Message, dto.ReplyId);
            if (result.Status)
            {
                var sourceId = dto.SourceId;
                //更新评论数
                if (sourceId.StartsWith("topic_")) //帖子
                {
                    sourceId = sourceId.Replace("topic_", "").Trim();
                    CurrentIocManager.Resolve<ITopicContract>().UpdateTopicReplyNum(sourceId);
                }
                else
                {
                    CurrentIocManager.Resolve<IMessageContract>().UpdateDynamicCommentCount(sourceId, 1);
                }
            }
            return result;
        }
    }
}
