
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 评论相关契约 </summary>
    public interface ICommentContract : IDependency
    {
        /// <summary> 发表评论 </summary>
        /// <param name="sourceId"></param>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <param name="replyId"></param>
        /// <returns></returns>
        DResult<CommentDto> Comment(string sourceId, long userId, string message, string replyId = null);

        /// <summary> 删除评论 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<int> Delete(string id, long userId);

        /// <summary> 删除整个源的评论 </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        DResult Delete(string sourceId);

        /// <summary> 喜欢评论 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<int> Like(string id, long userId);

        /// <summary> 厌恶评论 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<int> Hate(string id, long userId);

        /// <summary> 评论数 </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        long CommentCount(string sourceId);

        /// <summary> 评论列表 </summary>
        /// <param name="searchDto"></param>
        /// <returns></returns>
        DResult<CommentResultDto> CommentList(CommentSearchDto searchDto);

        /// <summary> 评论对话 </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DResult<CommentResultDto> CommentDialog(string id);
    }
}
