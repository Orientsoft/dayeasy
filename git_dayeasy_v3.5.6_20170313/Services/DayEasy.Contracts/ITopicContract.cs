using System;
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Topic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary>
    /// 帖子部分接口
    /// </summary>
    public partial interface ITopicContract : IDependency
    {
        DResults<TopicDto> GetTopics(SearchTopicDto searchDto);

        TopicDto GetTopicDetail(string topicId, long userId);

        VoteDto GetTopicVote(string topicId, long userId);

        DResults<UserDto> GetTopicPraiseUser(string topicId);

        DResult DeleteTopic(string topicId, long userId);

        DResult DeleteVote(string topicId, long userId);

        void UpdateTopicReadNum(string topicId);

        DResult UpdateTopicReplyNum(string topicId, bool isDelete = false);

        DResult UpdateTopicState(string topicId, long userId, TopicState state);

        DResult UpdateTopicPraise(string topicId, long userId, bool isPraise);

        DResult PublishTopic(PubTopicDto pubTopic);

        DResult EditTopic(string topicId, PubTopicDto pubTopic);

        DResult CastVote(string voteId, long userId, List<string> voteOptions);

        /// <summary> 更新的帖子数 </summary>
        /// <param name="groupId"></param>
        /// <param name="lastvisit"></param>
        /// <returns></returns>
        int UpdateCount(string groupId, DateTime? lastvisit);

        Dictionary<string, int> UpdateCountDict(Dictionary<string, DateTime?> groupDict);
    }
}
