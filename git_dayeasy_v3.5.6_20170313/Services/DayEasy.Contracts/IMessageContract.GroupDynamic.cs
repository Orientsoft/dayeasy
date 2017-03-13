
using System;
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 消息/动态类契约 - 圈子动态 </summary>
    public partial interface IMessageContract
    {
        /// <summary> 发送动态消息 </summary>
        /// <param name="groupDynamicDto"></param>
        /// <returns></returns>
        DResult SendDynamic(DynamicSendDto groupDynamicDto);

        /// <summary> 批量发送动态 </summary>
        /// <param name="dynamicSendDtos"></param>
        /// <returns></returns>
        DResult<int> SendDynamics(IEnumerable<DynamicSendDto> dynamicSendDtos);

        /// <summary> 删除动态 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult DeleteDynamic(string id, long userId);

        /// <summary> 点赞/取消点赞 </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<int> LikeDynamic(string id, long userId);

        /// <summary> 更新评论数 </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        DResult UpdateDynamicCommentCount(string id, int count);

        /// <summary> 获取动态 </summary>
        /// <param name="searchDto"></param>
        /// <returns></returns>
        DResult<DynamicMessageResultDto> GetDynamics(DynamicSearchDto searchDto);

        /// <summary> 最新动态数 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <param name="lastUpdate"></param>
        /// <returns></returns>
        int DynamicCount(string groupId, long userId, byte role, DateTime? lastUpdate);

        /// <summary> 最新动态数 </summary>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <param name="groupInfo"></param>
        /// <returns></returns>
        Dictionary<string, int> DynamicCountDict(long userId, byte role, Dictionary<string, DateTime?> groupInfo);
    }
}
