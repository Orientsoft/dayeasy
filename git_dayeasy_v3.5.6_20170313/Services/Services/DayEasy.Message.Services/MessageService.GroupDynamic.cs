using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Message.Services
{
    public partial class MessageService
    {
        public IVersion3Repository<TM_GroupDynamic, string> GroupDynamicRepository { private get; set; }

        public IVersion3Repository<TG_Member, string> MemberRepository { private get; set; }
        public IVersion3Repository<TG_Group, string> GroupRepository { private get; set; }

        private TM_GroupDynamic ToDynamic(DynamicSendDto groupDynamicDto)
        {
            if (groupDynamicDto == null)
                return null;
            var dynamic = new TM_GroupDynamic
            {
                Id = IdHelper.Instance.Guid32,
                GroupId = groupDynamicDto.GroupId,
                DynamicType = (byte)groupDynamicDto.DynamicType,
                ContentType = groupDynamicDto.ContentType,
                ContentId = groupDynamicDto.ContentId,
                Status = (byte)NormalStatus.Normal,
                ReceiveRole = (int)groupDynamicDto.ReceivRole,
                CommentCount = 0,
                GoodCount = 0,
                Title = groupDynamicDto.Title,
                Message = groupDynamicDto.Message.FormatMessage(),
                AddedBy = groupDynamicDto.UserId,
                AddedAt = Clock.Now
            };

            if (groupDynamicDto.RecieveIds != null && groupDynamicDto.RecieveIds.Any())
                dynamic.ReceiverIds = groupDynamicDto.RecieveIds.ToJson();
            return dynamic;
        }

        public DResult SendDynamic(DynamicSendDto groupDynamicDto)
        {
            if (groupDynamicDto == null || string.IsNullOrWhiteSpace(groupDynamicDto.GroupId))
                return DResult.Error("发送消息失败！");
            if (groupDynamicDto.DynamicType == GroupDynamicType.Notice &&
                string.IsNullOrWhiteSpace(groupDynamicDto.Message))
            {
                return DResult.Error("通知内容不能为空！");
            }
            var dynamic = ToDynamic(groupDynamicDto);

            var result = GroupDynamicRepository.Insert(dynamic);

            if (!string.IsNullOrWhiteSpace(result))
                return DResult.Success;
            return DResult.Error("发送消息失败！");
        }

        public DResult<int> SendDynamics(IEnumerable<DynamicSendDto> dynamicSendDtos)
        {
            var dynamics = new List<TM_GroupDynamic>();
            foreach (var dynamicSendDto in dynamicSendDtos)
            {
                var dynamic = ToDynamic(dynamicSendDto);
                if (dynamic != null)
                    dynamics.Add(dynamic);
            }
            var result = GroupDynamicRepository.Insert(dynamics);
            return result > 0 ? DResult.Succ(result) : DResult.Error<int>("发布失败！");
        }

        public DResult DeleteDynamic(string id, long userId)
        {
            var dynamic = GroupDynamicRepository.Load(id);
            if (dynamic == null || dynamic.Status == (byte)NormalStatus.Delete)
                return DResult.Error("动态不存在！");
            if (dynamic.AddedBy != userId)
                return DResult.Error("你不能删除该动态！");
            dynamic.Status = (byte)NormalStatus.Delete;
            var result = GroupDynamicRepository.Update(d => new { d.Status }, dynamic);
            return DResult.FromResult(result);
        }

        public DResult<int> LikeDynamic(string id, long userId)
        {
            var dynamic = GroupDynamicRepository.Load(id);
            if (dynamic == null || dynamic.Status == (byte)NormalStatus.Delete)
                return DResult.Error<int>("动态不存在！");
            var likes = string.IsNullOrWhiteSpace(dynamic.Goods)
                ? new List<long>()
                : dynamic.Goods.JsonToObject<List<long>>();
            if (likes.Contains(userId))
            {
                //取消点赞
                likes.Remove(userId);
                dynamic.GoodCount--;
            }
            else
            {
                likes.Add(userId);
                dynamic.GoodCount++;
            }
            dynamic.Goods = likes.ToJson();
            if (dynamic.GoodCount < 0)
                dynamic.GoodCount = 0;
            var result = GroupDynamicRepository.Update(d => new
            {
                d.GoodCount,
                d.Goods
            }, dynamic);
            return result > 0 ? DResult.Succ(dynamic.GoodCount) : DResult.Error<int>("点赞失败！");
        }

        public DResult UpdateDynamicCommentCount(string id, int count)
        {
            var dynamic = GroupDynamicRepository.Load(id);
            if (dynamic == null)
                return DResult.Error("动态不存在！");
            dynamic.CommentCount += count;
            if (dynamic.CommentCount < 0)
                dynamic.CommentCount = 0;
            GroupDynamicRepository.Update(d => new { d.CommentCount }, dynamic);
            return DResult.Success;
        }

        #region 获取消息

        /// <summary> 获取消息 </summary>
        /// <param name="searchDto"></param>
        /// <returns></returns>
        public DResult<DynamicMessageResultDto> GetDynamics(DynamicSearchDto searchDto)
        {
            if (searchDto == null || string.IsNullOrWhiteSpace(searchDto.GroupId)
                || searchDto.UserId <= 0)
                return DResult.Error<DynamicMessageResultDto>("获取动态异常！");
            var userIdStr = searchDto.UserId.ToString(CultureInfo.InvariantCulture);
            var member =
                MemberRepository.FirstOrDefault(
                    t =>
                        t.GroupId == searchDto.GroupId
                        && t.MemberId == searchDto.UserId
                        && t.Status == (byte)(NormalStatus.Normal));
            //            if (member == null)
            //                return DResult.Error<DynamicMessageResultDto>("你还不是圈子成员，不能获取圈内动态！");
            DateTime? registTime = null;
            var groupType =
                GroupRepository.Where(g => g.Id == searchDto.GroupId).Select(g => g.GroupType).FirstOrDefault();
            if (member != null && groupType != (byte)GroupType.Colleague)
                registTime = member.AddedAt;

            Expression<Func<TM_GroupDynamic, bool>> condition =
                d => d.GroupId == searchDto.GroupId
                     && d.Status == (byte)NormalStatus.Normal
                     && (d.AddedBy == searchDto.UserId || (d.ReceiveRole & (byte)searchDto.Role) > 0
                         && (d.ReceiverIds == null || (d.ReceiverIds.Contains(userIdStr))));

            if (registTime.HasValue)
            {
                condition = condition.And(d => d.AddedAt >= registTime.Value);
            }
            if (searchDto.Type >= 0)
            {
                condition = condition.And(t => t.DynamicType == searchDto.Type);
            }

            var msgs = GroupDynamicRepository.Where(condition);

            var newsList =
                msgs.OrderByDescending(u => u.AddedAt)
                    .Skip(searchDto.Page * searchDto.Size)
                    .Take(searchDto.Size).DistinctBy(w=>w.ContentId)
                    .ToList();
            var totalCount = msgs.Count();

            if (newsList.Count <= 0)
                return DResult.Succ(new DynamicMessageResultDto());
            var userId = (searchDto.ParentId > 0 ? searchDto.ParentId : searchDto.UserId);
            var newsDetailList =
                newsList.Select(news => GetNewsDetail(userId, news, searchDto.Role))
                    .Where(newsModel => newsModel != null)
                    .ToList();

            //前台显示消息模型
            var dynamicNews = new DynamicMessageResultDto
            {
                UserRole = (byte)searchDto.Role,
                TotalCount = totalCount,
                NewsDetails = newsDetailList
            };
            var userIds = newsDetailList.Select(t => t.UserId);
            var goods = newsDetailList.SelectMany(t => t.Goods ?? new List<long>());
            userIds = userIds.Union(goods).Distinct();
            dynamicNews.Users = UserContract.LoadDList(userIds.ToList()).ToDictionary(k => k.Id, v => v);
            return DResult.Succ(dynamicNews);
        }

        /// <summary> 动态数 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <param name="lastUpdate"></param>
        /// <returns></returns>
        public int DynamicCount(string groupId, long userId, byte role, DateTime? lastUpdate)
        {
            if (string.IsNullOrWhiteSpace(groupId) || userId <= 0)
                return 0;
            var idStr = userId.ToString();
            Expression<Func<TM_GroupDynamic, bool>> condition =
                d => d.GroupId == groupId && d.Status == (byte)NormalStatus.Normal
                     && (d.AddedBy == userId ||
                         (d.ReceiveRole & role) > 0 && (d.ReceiverIds == null || d.ReceiverIds.Contains(idStr)));
            if (lastUpdate.HasValue)
                condition = condition.And(d => d.AddedAt > lastUpdate);
            return GroupDynamicRepository.Count(condition);
        }

        public Dictionary<string, int> DynamicCountDict(long userId, byte role, Dictionary<string, DateTime?> groupInfo)
        {
            var dict = new Dictionary<string, int>();
            if (userId <= 0 || groupInfo == null || !groupInfo.Any())
                return dict;
            var idStr = userId.ToString();

            Expression<Func<TM_GroupDynamic, bool>> condition =
                d => d.Status == (byte)NormalStatus.Normal
                     &&
                     (d.AddedBy == userId ||
                      (d.ReceiveRole & role) > 0 && (d.ReceiverIds == null || d.ReceiverIds.Contains(idStr)));
            Expression<Func<TM_GroupDynamic, bool>> groupCondition = d => false;
            foreach (var groupId in groupInfo.Keys)
            {
                var id = groupId;
                var lastTime = groupInfo[id];
                Expression<Func<TM_GroupDynamic, bool>> itemCondition = d => d.GroupId == id;
                if (lastTime.HasValue)
                {
                    itemCondition = itemCondition.And(d => d.AddedAt > lastTime.Value);
                }
                groupCondition = groupCondition.Or(itemCondition);
            }
            condition = condition.And(groupCondition);
            return GroupDynamicRepository.Where(condition).Select(t => t.GroupId)
                .ToList()
                .GroupBy(d => d)
                .ToDictionary(k => k.Key, v => v.Count());
        }

        #endregion
    }
}
