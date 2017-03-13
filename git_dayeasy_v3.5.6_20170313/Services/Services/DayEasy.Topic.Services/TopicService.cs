using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Topic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.EntityFramework;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;

namespace DayEasy.Topic.Services
{
    public partial class TopicService : Version3Service, ITopicContract
    {
        private readonly ILogger _logger = LogManager.Logger<TopicService>();
        public TopicService(IDbContextProvider<Version3DbContext> context) : base(context) { }

        public IUserContract UserContract { private get; set; }
        public IVersion3Repository<TG_Group> GroupRepository { private get; set; }
        public IVersion3Repository<TG_Share> GroupShareRepository { private get; set; }
        public IVersion3Repository<TG_Member> MemberRepository { private get; set; }
        public IVersion3Repository<TB_Topic> TopicRepository { private get; set; }
        public IVersion3Repository<TB_Vote> VoteRepository { private get; set; }
        public IVersion3Repository<TB_VoteOption> VoteOptionRepository { private get; set; }
        public IVersion3Repository<TB_VoteUser> VoteUserRepository { private get; set; }
        public IVersion3Repository<TB_Praise> PraiseRepository { private get; set; }

        #region 查询帖子列表
        /// <summary>
        /// 查询帖子列表
        /// </summary>
        /// <returns></returns>
        public DResults<TopicDto> GetTopics(SearchTopicDto searchDto)
        {
            if (searchDto == null) return DResult.Errors<TopicDto>("参数错误！");

            var topics = TopicRepository.Where(u => u.Status < (byte)TopicStatus.Delete);

            if (searchDto.ClassType > -1)
            {
                if (searchDto.ClassType % 100 == 0)//顶级
                {
                    var type = searchDto.ClassType / 100;

                    topics = topics.Where(u => u.ClassType >= type * 100 && u.ClassType < (type + 1) * 100);
                }
                else
                {
                    topics = topics.Where(u => u.ClassType == searchDto.ClassType);
                }
            }

            if (searchDto.ExceptIds != null && searchDto.ExceptIds.Count > 0)
            {
                topics = topics.Where(u => !searchDto.ExceptIds.Contains(u.Id));
            }

            if (searchDto.Tags != null && searchDto.Tags.Count > 0)
            {
                var firstTag = searchDto.Tags.First();
                Expression<Func<TB_Topic, bool>> tagCondition = u => u.Tags.Contains(firstTag);
                searchDto.Tags.RemoveAt(0);
                searchDto.Tags.Foreach(t =>
                {
                    tagCondition = tagCondition.Or(u => u.Tags.Contains(t));
                });
                if (topics.Where(tagCondition).Any())
                    topics = topics.Where(tagCondition);
            }

            if (searchDto.State != (byte)TopicState.Normal && Enum.IsDefined(typeof(TopicState), (byte)searchDto.State))
            {
                topics = topics.Where(u => (u.State & searchDto.State) > 0);
            }

            if (string.IsNullOrEmpty(searchDto.GroupId))
            {
                topics = topics.Join(GroupShareRepository.Where(u => u.JoinAuth == (byte)GroupJoinAuth.Public), t => t.GroupId, g => g.Id, (t, g) => t);
                switch (searchDto.Order)
                {
                    case TopicOrder.TimeAsc:
                        topics = topics.OrderBy(u => u.AddedAt);
                        break;
                    case TopicOrder.PraiseNum:
                        topics = topics.OrderByDescending(u => u.PraiseNum);
                        break;
                    case TopicOrder.ReadNum:
                        topics = topics.OrderByDescending(u => u.ReadNum);
                        break;
                    case TopicOrder.ReplyNum:
                        topics = topics.OrderByDescending(u => u.ReplyNum);
                        break;
                    default:
                        topics = topics.OrderByDescending(u => u.AddedAt);
                        break;
                }
            }
            else
            {
                topics = topics.Where(u => u.GroupId == searchDto.GroupId);
                switch (searchDto.Order)
                {
                    case TopicOrder.TimeAsc:
                        topics = topics.OrderByDescending(u => (u.State & (byte)TopicState.Recommend)).ThenBy(u => u.AddedAt);
                        break;
                    case TopicOrder.PraiseNum:
                        topics = topics.OrderByDescending(u => (u.State & (byte)TopicState.Recommend)).ThenByDescending(u => u.PraiseNum);
                        break;
                    case TopicOrder.ReadNum:
                        topics = topics.OrderByDescending(u => (u.State & (byte)TopicState.Recommend)).ThenByDescending(u => u.ReadNum);
                        break;
                    case TopicOrder.ReplyNum:
                        topics = topics.OrderByDescending(u => (u.State & (byte)TopicState.Recommend)).ThenByDescending(u => u.ReplyNum);
                        break;
                    default:
                        topics = topics.OrderByDescending(u => (u.State & (byte)TopicState.Recommend)).ThenByDescending(u => u.AddedAt);
                        break;
                }
            }

            var data = topics.Skip(searchDto.Size * searchDto.Page).Take(searchDto.Size).ToList();
            var count = topics.Count();

            var result = data.MapTo<List<TopicDto>>();
            if (result == null) return DResult.Succ<TopicDto>(null, 0);

            var userIds = data.Select(u => u.AddedBy).ToList();
            var users = UserContract.LoadList(userIds);
            if (users != null && users.Count > 0)
            {
                result.Foreach(d =>
                {
                    var user = users.SingleOrDefault(u => u.Id == d.AddedBy);
                    if (user != null)
                    {
                        d.UserName = user.Nick;
                        d.UserPhoto = user.Avatar;
                    }
                });
            }

            return DResult.Succ(result, count);
        }

        #endregion

        #region 获取帖子详情
        /// <summary>
        /// 获取帖子详情
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TopicDto GetTopicDetail(string topicId, long userId)
        {
            if (string.IsNullOrEmpty(topicId)) return null;

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId && u.Status < (byte)TopicStatus.Delete);

            if (topic == null) return null;

            var result = topic.MapTo<TopicDto>();

            if (result == null) return null;

            //查询投票
            if (result.HasVote)
            {
                result.VoteDetail = GetTopicVote(result.Id, userId);
            }

            //查询用户名
            var user = UserContract.Load(topic.AddedBy);
            if (user != null)
            {
                result.UserName = user.Nick;
                result.UserPhoto = user.Avatar;
            }

            //查询是否已经点赞
            if (userId > 0)
                result.hadPraised = PraiseRepository.Exists(u => u.TopicId == topic.Id && u.UserId == userId);

            return result;
        }
        #endregion

        #region 查询帖子的投票
        /// <summary>
        /// 查询帖子的投票
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public VoteDto GetTopicVote(string topicId, long userId)
        {
            if (string.IsNullOrEmpty(topicId)) return null;

            var vote = VoteRepository.SingleOrDefault(u => u.TopicId == topicId && u.Status == (byte)NormalStatus.Normal);
            if (vote == null) return null;

            var voteOptions = VoteOptionRepository.Where(u => u.VoteId == vote.Id).ToList();
            if (voteOptions.Count <= 0) return null;

            var voteDto = vote.MapTo<VoteDto>();
            if (voteDto == null) return null;

            voteDto.VoteOptions = voteOptions.MapTo<List<VoteOptionDto>>();
            //查询是否已经投票
            if (userId > 0)
            {
                voteDto.HadVoted = VoteUserRepository.Exists(u => u.UserId == userId && u.VoteId == vote.Id);
            }

            return voteDto;
        }
        #endregion

        #region 获取帖子点赞的用户
        /// <summary>
        /// 获取帖子点赞的用户
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>

        public DResults<UserDto> GetTopicPraiseUser(string topicId)
        {
            if (string.IsNullOrEmpty(topicId)) return DResult.Errors<UserDto>("参数错误！");

            var userIds = PraiseRepository.Where(u => u.TopicId == topicId).Select(u => u.UserId).ToList();
            if (userIds.Count <= 0) return null;

            var users = UserContract.LoadList(userIds);
            return DResult.Succ(users, users.Count);
        }
        #endregion

        #region 删除帖子
        /// <summary>
        /// 删除帖子
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult DeleteTopic(string topicId, long userId)
        {
            if (string.IsNullOrEmpty(topicId)) return DResult.Error("参数错误！");

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId);
            if (topic == null || topic.Status == (byte)TopicStatus.Delete) return DResult.Success;

            if (topic.AddedBy != userId)
            {
                var isManager = GroupRepository.Exists(u => u.Id == topic.GroupId && u.ManagerId == userId);//是否是圈主
                if (!isManager)
                    return DResult.Error("你没有删除该帖子的权限！");
            }

            topic.Status = (byte)TopicStatus.Delete;

            var shareGroup = GroupShareRepository.SingleOrDefault(u => u.Id == topic.GroupId);
            var member = MemberRepository.SingleOrDefault(u => u.MemberId == topic.AddedBy && u.GroupId == topic.GroupId);

            var result = UnitOfWork.Transaction(() =>
            {
                TopicRepository.Update(t => new { t.Status }, topic);

                //圈子帖子总数量--
                if (shareGroup != null)
                {
                    shareGroup.TopicNum--;
                    if (shareGroup.TopicNum < 0)
                    {
                        shareGroup.TopicNum = 0;
                    }
                    GroupShareRepository.Update(t => new { t.TopicNum }, shareGroup);
                }

                //个人帖子总数量--
                if (member != null)
                {
                    member.TopicNum--;
                    if (member.TopicNum < 0)
                    {
                        member.TopicNum = 0;
                    }

                    MemberRepository.Update(t => new { t.TopicNum }, member);
                }
            });

            return result > 0 ? DResult.Success : DResult.Error("删除失败，请稍候重试！");
        }
        #endregion

        #region 删除帖子的投票
        /// <summary>
        /// 删除帖子的投票
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult DeleteVote(string topicId, long userId)
        {
            if (string.IsNullOrEmpty(topicId)) return DResult.Error("参数错误！");

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId && u.Status < (byte)TopicStatus.Delete);
            if (topic == null) return DResult.Error("参数错误！");

            var vote = VoteRepository.SingleOrDefault(u => u.TopicId == topicId && u.AddedBy == userId && u.Status == (byte)NormalStatus.Normal);

            if (vote == null || vote.Status == (byte)NormalStatus.Delete) return DResult.Success;

            vote.Status = (byte)NormalStatus.Delete;
            topic.HasVote = false;

            var result = UnitOfWork.Transaction(() =>
            {
                VoteRepository.Update(t => new { t.Status }, vote);
                TopicRepository.Update(t => new { t.HasVote }, topic);
            });

            return result > 0 ? DResult.Success : DResult.Error("删除失败，请稍后重试！");
        }
        #endregion

        #region 更新帖子的阅读量
        /// <summary>
        /// 更新帖子的阅读量
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public void UpdateTopicReadNum(string topicId)
        {
            if (string.IsNullOrEmpty(topicId)) return;

            Task.Factory.StartNew(() =>
            {
                var topicRepository = CurrentIocManager.Resolve<IVersion3Repository<TB_Topic>>();
                var topic = topicRepository.SingleOrDefault(u => u.Id == topicId && u.Status < (byte)TopicStatus.Delete);

                if (topic == null) return;

                topic.ReadNum++;

                topicRepository.Update(t => new { t.ReadNum }, topic);
            });
        }
        #endregion

        #region 更新帖子的回复数量
        /// <summary>
        /// 更新帖子的回复数量
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        public DResult UpdateTopicReplyNum(string topicId, bool isDelete = false)
        {
            if (string.IsNullOrEmpty(topicId)) return DResult.Error("参数错误！");

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId && u.Status < (byte)TopicStatus.Delete);

            if (topic == null) return DResult.Error("该帖子不存在！");

            if (isDelete)
            {
                topic.ReplyNum--;

                if (topic.ReplyNum < 0)
                {
                    topic.ReplyNum = 0;
                }
            }
            else
            {
                topic.ReplyNum++;
            }

            var result = TopicRepository.Update(t => new { t.ReplyNum }, topic);

            return result > 0 ? DResult.Success : DResult.Error("更新失败，请稍后重试！");
        }
        #endregion

        #region 点精 or 取消点精 or 置顶 or  取消置顶

        /// <summary>
        /// 点精 or 取消点精 or 置顶 or  取消置顶
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="userId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public DResult UpdateTopicState(string topicId, long userId, TopicState state)
        {
            if (string.IsNullOrEmpty(topicId)) return DResult.Error("参数错误！");

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId && u.Status < (byte)TopicStatus.Delete);

            if (topic == null)
                return DResult.Error("该帖子不存在！");
            //只有圈主能加精   --add by shay
            var group = GroupRepository.Load(topic.GroupId);
            if (group == null || group.ManagerId != userId)
                return DResult.Error("您没有操作权限！");

            topic.State ^= (byte)state;

            var result = TopicRepository.Update(t => new { t.State }, topic);

            return result > 0 ? DResult.Success : DResult.Error("操作失败，请稍候重试！");
        }

        #endregion

        #region 点赞 or 取消点赞
        /// <summary>
        /// 点赞 or 取消点赞
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="userId"></param>
        /// <param name="isPraise"></param>
        /// <returns></returns>
        public DResult UpdateTopicPraise(string topicId, long userId, bool isPraise)
        {
            if (string.IsNullOrEmpty(topicId)) return DResult.Error("参数错误！");

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId && u.Status < (byte)TopicStatus.Delete);

            if (topic == null)
                return DResult.Error("该帖子不存在！");

            var result = 0;
            var praisedModel = PraiseRepository.SingleOrDefault(u => u.TopicId == topicId && u.UserId == userId);
            if (isPraise) //点赞
            {
                if (praisedModel != null)
                    return DResult.Error("您已经点过赞了！");

                topic.PraiseNum++;

                var praiseModel = new TB_Praise()
                {
                    Id = IdHelper.Instance.Guid32,
                    TopicId = topicId,
                    UserId = userId
                };

                result = TopicRepository.UnitOfWork.Transaction(() =>
                {
                    TopicRepository.Update(t => new { t.PraiseNum }, topic);
                    PraiseRepository.Insert(praiseModel);
                });
            }
            else //取消点赞
            {
                if (praisedModel == null)
                    return DResult.Success;

                topic.PraiseNum--;

                if (topic.PraiseNum < 0)
                {
                    topic.PraiseNum = 0;
                }

                result = TopicRepository.UnitOfWork.Transaction(() =>
                {
                    TopicRepository.Update(t => new { t.PraiseNum }, topic);
                    PraiseRepository.Delete(praisedModel);
                });
            }

            return DResult.FromResult(result);
        }
        #endregion

        #region 发帖子
        /// <summary>
        /// 发帖子
        /// </summary>
        /// <param name="pubTopic"></param>
        /// <returns></returns>
        public DResult PublishTopic(PubTopicDto pubTopic)
        {
            if (pubTopic == null || string.IsNullOrEmpty(pubTopic.GroupId))
                return DResult.Error("参数错误！");

            var group = GroupRepository.SingleOrDefault(u => u.Id == pubTopic.GroupId && u.GroupType == (byte)GroupType.Share && u.Status == (byte)NormalStatus.Normal);
            if (group == null)
                return DResult.Error("没有发帖权限！");

            var shareGroup = GroupShareRepository.SingleOrDefault(u => u.Id == group.Id);
            if (shareGroup == null)
                return DResult.Error("没有发帖权限！");

            if (group.ManagerId != pubTopic.UserId && shareGroup.PostAuth == (byte)GroupPostAuth.Owner)
                return DResult.Error("没有发帖权限！");

            var member = MemberRepository.SingleOrDefault(u => u.GroupId == group.Id && u.MemberId == pubTopic.UserId && u.Status == (byte)CheckStatus.Normal);
            if (member == null)
                return DResult.Error("没有发帖权限！");

            if (string.IsNullOrEmpty(pubTopic.Title))
                return DResult.Error("请输入标题！");

            if (pubTopic.Title.Length > 250)
                return DResult.Error("标题最多250个字！");

            if (string.IsNullOrEmpty(pubTopic.Content))
                return DResult.Error("请输入帖子内容！");

            #region 组装数据
            var topicModel = new TB_Topic()
                {
                    Id = IdHelper.Instance.Guid32,
                    AddedAt = Clock.Now,
                    AddedBy = pubTopic.UserId,
                    ClassType = shareGroup.ClassType,
                    Content = pubTopic.Content.HtmlDecode(),
                    GroupId = pubTopic.GroupId,
                    PraiseNum = 0,
                    ReadNum = 0,
                    ReplyNum = 0,
                    State = (byte)TopicState.Normal,
                    Status = (byte)TopicStatus.Normal,
                    Tags = pubTopic.Tags.ToJson(),
                    Title = pubTopic.Title,
                    HasVote = pubTopic.PubVote != null && pubTopic.PubVote.VoteOptions != null && pubTopic.PubVote.VoteOptions.Count > 0
                };

            TB_Vote voteModel = null;
            List<TB_VoteOption> voteOptionList = null;

            if (pubTopic.PubVote != null && pubTopic.PubVote.VoteOptions != null && pubTopic.PubVote.VoteOptions.Count > 0)
            {
                voteModel = new TB_Vote()
                {
                    AddedAt = Clock.Now,
                    AddedBy = pubTopic.UserId,
                    FinishedAt = pubTopic.PubVote.FinishedAt,
                    Id = IdHelper.Instance.Guid32,
                    ImgUrl = pubTopic.PubVote.ImgUrl,
                    IsPublic = pubTopic.PubVote.IsPublic,
                    IsSingleSelection = pubTopic.PubVote.IsSingle,
                    Status = (byte)NormalStatus.Normal,
                    Title = pubTopic.PubVote.Title,
                    TopicId = topicModel.Id
                };

                voteOptionList = pubTopic.PubVote.VoteOptions.Select(option => new TB_VoteOption()
                {
                    Id = IdHelper.Instance.Guid32,
                    Count = 0,
                    OptionContent = option.OpContent,
                    Sort = option.Sort,
                    VoteId = voteModel.Id
                }).ToList();
            }
            #endregion

            var result = TopicRepository.UnitOfWork.Transaction(() =>
            {
                TopicRepository.Insert(topicModel);

                //更新圈子帖子总数量
                shareGroup.TopicNum++;
                GroupShareRepository.Update(shareGroup);

                //更新个人帖子总数量
                member.TopicNum++;
                MemberRepository.Update(member);

                if (voteModel == null) return;

                VoteRepository.Insert(voteModel);
                if (voteOptionList != null && voteOptionList.Count > 0)
                {
                    VoteOptionRepository.Insert(voteOptionList);
                }
            });

            return result > 0 ? DResult.Success : DResult.Error("发布失败，请稍后重试！");
        }
        #endregion

        #region 编辑帖子

        /// <summary>
        /// 编辑帖子
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="pubTopic"></param>
        /// <returns></returns>
        public DResult EditTopic(string topicId, PubTopicDto pubTopic)
        {
            if (string.IsNullOrEmpty(topicId) || pubTopic == null) return DResult.Error("参数错误！");

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId && u.Status < (byte)TopicStatus.Delete);

            if (topic == null)
                return DResult.Error("该帖子不存在！");

            topic.Title = pubTopic.Title;
            topic.Content = pubTopic.Content.HtmlDecode();
            topic.Tags = pubTopic.Tags.ToJson();

            if (pubTopic.PubVote != null && pubTopic.PubVote.VoteOptions != null &&
                pubTopic.PubVote.VoteOptions.Count > 0)
                topic.HasVote = true;

            TB_Vote voteModel = null;
            List<TB_VoteOption> voteOptionList = null;

            if (pubTopic.PubVote != null && pubTopic.PubVote.VoteOptions != null &&
                pubTopic.PubVote.VoteOptions.Count > 0)
            {
                voteModel = new TB_Vote
                {
                    AddedAt = Clock.Now,
                    AddedBy = pubTopic.UserId,
                    FinishedAt = pubTopic.PubVote.FinishedAt,
                    Id = IdHelper.Instance.Guid32,
                    ImgUrl = pubTopic.PubVote.ImgUrl,
                    IsPublic = pubTopic.PubVote.IsPublic,
                    IsSingleSelection = pubTopic.PubVote.IsSingle,
                    Status = (byte)NormalStatus.Normal,
                    Title = pubTopic.PubVote.Title,
                    TopicId = topic.Id
                };

                voteOptionList = pubTopic.PubVote.VoteOptions.Select(option => new TB_VoteOption
                {
                    Id = IdHelper.Instance.Guid32,
                    Count = 0,
                    OptionContent = option.OpContent,
                    Sort = option.Sort,
                    VoteId = voteModel.Id
                }).ToList();
            }

            var result = UnitOfWork.Transaction(() =>
            {
                TopicRepository.Update(t => new
                {
                    t.Title,
                    t.Content,
                    t.Tags,
                    t.HasVote
                }, topic);
                if (voteModel == null) return;

                VoteRepository.Insert(voteModel);
                if (voteOptionList != null && voteOptionList.Count > 0)
                {
                    VoteOptionRepository.Insert(voteOptionList);
                }
            });

            return result > 0 ? DResult.Success : DResult.Error("编辑失败，请稍后重试！");
        }

        #endregion

        #region 保存投票
        /// <summary>
        /// 保存投票
        /// </summary>
        /// <param name="voteId"></param>
        /// <param name="userId"></param>
        /// <param name="voteOptions"></param>
        /// <returns></returns>
        public DResult CastVote(string voteId, long userId, List<string> voteOptions)
        {
            if (string.IsNullOrEmpty(voteId) || userId < 1 || voteOptions == null || voteOptions.Count < 1)
                return DResult.Error("参数错误！");

            //查询是否已经投票
            var exists = VoteUserRepository.Exists(u => u.UserId == userId && u.VoteId == voteId);
            if (exists)
                return DResult.Error("你已经投过票了！");

            //验证投票与选项
            var vote = VoteRepository.SingleOrDefault(u => u.Id == voteId && u.Status == (byte)NormalStatus.Normal);
            if (vote == null)
                return DResult.Error("该投票不存在！");

            if (vote.FinishedAt < DateTime.Now)
                return DResult.Error("该投票已经结束了！");

            if (vote.IsSingleSelection && voteOptions.Count > 1)
                return DResult.Error("该投票只能选择一项！");

            var voteItems = VoteOptionRepository.Where(u => u.VoteId == voteId && voteOptions.Contains(u.Id)).ToList();
            if (voteItems.Count < 1 || voteItems.Count != voteOptions.Count)
                return DResult.Error("投票数据异常！");

            voteItems.Foreach(u => u.Count++);//数量++

            var voteUsers = new List<TB_VoteUser>();
            voteItems.Foreach(v =>
            {
                var voteUser = new TB_VoteUser
                {
                    Id = IdHelper.Instance.Guid32,
                    AddedAt = Clock.Now,
                    UserId = userId,
                    VoteId = vote.Id,
                    VoteOptionId = v.Id
                };

                voteUsers.Add(voteUser);
            });

            var result = VoteUserRepository.UnitOfWork.Transaction(() =>
            {
                voteItems.Foreach(v => VoteOptionRepository.Update(v));
                VoteUserRepository.Insert(voteUsers);
            });

            return result > 0 ? DResult.Success : DResult.Error("投票失败，请稍后重试！");
        }

        public int UpdateCount(string groupId, DateTime? lastvisit)
        {
            return TopicRepository.Count(t => t.GroupId == groupId && (!lastvisit.HasValue || t.AddedAt > lastvisit));
        }

        public Dictionary<string, int> UpdateCountDict(Dictionary<string, DateTime?> groupDict)
        {
            var dict = new Dictionary<string, int>();
            if (groupDict == null || !groupDict.Any())
                return dict;
            Expression<Func<TB_Topic, bool>> condition = d => false;
            foreach (var groupId in groupDict.Keys)
            {
                var id = groupId;
                var time = groupDict[id];
                Expression<Func<TB_Topic, bool>> itemCondition = d => d.GroupId == id;
                if (time.HasValue)
                {
                    itemCondition = itemCondition.And(d => d.AddedAt > time.Value);
                }
                condition = condition.Or(itemCondition);
            }
            return TopicRepository.Where(condition)
                .Select(d => d.GroupId)
                .ToList()
                .GroupBy(d => d)
                .ToDictionary(k => k.Key, v => v.Count());
        }

        #endregion
    }
}
