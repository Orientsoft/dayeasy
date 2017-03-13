
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DayEasy.Group.Services
{
    /// <summary> 圈子成员相关业务 </summary>
    public partial class GroupService
    {
        public IVersion3Repository<TG_ApplyRecord> ApplyRecordRepository { private get; set; }
        public IUserContract UserContract { private get; set; }
        public IMessageContract MessageContract { private get; set; }

        #region 申请加入圈子

        /// <summary> 申请加入圈子 </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public DResult ApplyGroup(string groupId, long userId, string message, string name = null)
        {
            if (string.IsNullOrEmpty(groupId) || userId < 1)
                return DResult.Error("参数错误！");
            var status = IsGroupMember(userId, groupId);
            if (status == CheckStatus.Normal)
                return DResult.Error("你已经是圈子成员，无需再次申请！");
            if (status == CheckStatus.Pending)
                return DResult.Error("你的加入申请正在等待审核，请不要重复提交！");

            var groupInfo =
                GroupRepository.SingleOrDefault(u => u.Id == groupId && u.Status == (byte)NormalStatus.Normal);
            if (groupInfo == null)
                return DResult.Error("该圈子不存在！");
            var pendingCount =
                ApplyRecordRepository.Count(a => a.GroupId == groupId && a.Status == (byte)CheckStatus.Pending);

            if (groupInfo.MemberCount + pendingCount >= groupInfo.Capacity)
                return DResult.Error("该圈子已经满员！");
            var user = UserContract.Load(userId);
            if (user == null)
                return DResult.Error("用户Id不存在！");
            if (string.IsNullOrWhiteSpace(user.Name) && string.IsNullOrWhiteSpace(name) &&
                groupInfo.GroupType != (byte)GroupType.Share)
                return DResult.Error("申请需要填写真实姓名！");
            if (!string.IsNullOrWhiteSpace(name))
            {
                user.Name = name;
            }
            var check = AddGroupCheck(user, groupInfo);
            if (!check.Status)
                return check;

            var result = UnitOfWork.Transaction(() =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    UserContract.Update(new UserDto { Id = user.Id, Name = name });
                }
                //增加申请记录
                ApplyRecordRepository.Insert(new TG_ApplyRecord
                {
                    AddedAt = Clock.Now,
                    AddedBy = userId,
                    GroupId = groupId,
                    Id = IdHelper.Instance.Guid32,
                    MemberId = userId,
                    Message = message,
                    Status = (byte)CheckStatus.Pending
                });

                //更新圈子成员待审核人数统计
                groupInfo.UnCheckedCount += 1;
                GroupRepository.Update(g => new { g.UnCheckedCount }, groupInfo);
            });

            return result < 1 ? DResult.Error("申请失败，请稍后重试！") : DResult.Success;
        }

        #endregion

        #region 审核圈子申请

        /// <summary> 审核圈子申请 </summary>
        /// <param name="recordId"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public DResult Verify(string recordId, CheckStatus status, string message = null)
        {
            if (string.IsNullOrEmpty(recordId))
                return DResult.Error("参数错误！");

            var applyRecord =
                ApplyRecordRepository.SingleOrDefault(u => u.Id == recordId && u.Status == (byte)CheckStatus.Pending);
            if (applyRecord == null)
                return DResult.Error("未找到该审核记录，请稍后重试！");

            //减少圈子待审核数量
            var groupInfo =
                GroupRepository.SingleOrDefault(
                    u => u.Id == applyRecord.GroupId && u.Status == (byte)NormalStatus.Normal);
            if (groupInfo == null)
                return DResult.Error("该圈子已经不存在了！");
            var member = UserContract.Load(applyRecord.MemberId); //查询该成员在不在
            if (member == null)
                return DResult.Error("该申请人已经不存在了！");

            if (status == CheckStatus.Normal)
            {
                //通过审核时验证
                if (groupInfo.MemberCount + 1 > groupInfo.Capacity) //验证圈子容量
                    return DResult.Error("该圈子已满，不能加入了！");

                var check = AddGroupCheck(member, groupInfo, false);
                if (!check.Status)
                    return check;
            }

            //验证是否已经存在于该圈子了
            var memberInfo =
                MemberRepository.FirstOrDefault(
                    u => u.GroupId == applyRecord.GroupId && u.MemberId == applyRecord.MemberId);

            if (memberInfo != null && memberInfo.Status == (byte)NormalStatus.Normal)
                return DResult.Error("已经加入该圈子了！");

            var role = (member.IsTeacher() ? (byte)UserRole.Teacher : (byte)UserRole.Student);

            var result = UnitOfWork.Transaction(() =>
            {
                //如果通过，新增到成员表
                if (status == CheckStatus.Normal)
                {
                    groupInfo.MemberCount += 1; //圈子成员数量增加
                    if (memberInfo == null)
                    {
                        memberInfo = new TG_Member
                        {
                            AddedAt = Clock.Now,
                            BusinessCard = string.Empty,
                            GroupId = applyRecord.GroupId,
                            Id = IdHelper.Instance.Guid32,
                            MemberId = applyRecord.MemberId,
                            Status = (byte)NormalStatus.Normal,
                            MemberRole = role,
                            LastUpDateTime = Clock.Now
                        };
                        MemberRepository.Insert(memberInfo);
                    }
                    else
                    {
                        memberInfo.Status = (byte)NormalStatus.Normal;
                        MemberRepository.Update(m => new { m.Status }, memberInfo);
                    }
                }

                groupInfo.UnCheckedCount -= 1;
                if (groupInfo.UnCheckedCount < 0)
                {
                    groupInfo.UnCheckedCount = 0;
                }
                //减少圈子待审核数量
                GroupRepository.Update(g => new { g.MemberCount, g.UnCheckedCount }, groupInfo);

                applyRecord.Status = (byte)status;
                applyRecord.CheckedMessage = message;
                applyRecord.CheckedAt = Clock.Now;
                ApplyRecordRepository.Update(a => new { a.Status, a.CheckedMessage, a.CheckedAt }, applyRecord); //更新申请记录状态

                //发送系统消息
                string title;
                if (status == (byte)NormalStatus.Normal)
                {
                    title = "恭喜你，你已成功加入“{0}”".FormatWith(groupInfo.GroupName);
                }
                else
                {
                    title = "“{0}”拒绝了你的申请:{1}".FormatWith(groupInfo.GroupName, message);
                }
                MessageContract.SendMessage(new SystemMessageSendDto(applyRecord.MemberId)
                {
                    SenderId = groupInfo.ManagerId,
                    Title = title
                });
            });
            if (result <= 0)
                return DResult.Error("审核失败，请稍后重试！");
            return DResult.Success;
        }

        public DResult DeleteMember(string groupId, long memberId, long operateId)
        {
            if (string.IsNullOrWhiteSpace(groupId) || memberId <= 0 || operateId <= 0)
                return DResult.Error("参数错误，请重试！");
            var group = GroupRepository.Load(groupId);
            if (group == null)
                return DResult.Error("圈子不存在！");
            // 后台也可以移出成员 add by shay 2016-06-08
            //            if (group.ManagerId != operateId)
            //                return DResult.Error("只有圈主才能删除成员！");
            if (group.ManagerId == memberId)
                return DResult.Error("不能删除圈主！");
            var member = MemberRepository.FirstOrDefault(m => m.GroupId == groupId && m.MemberId == memberId);
            if (member == null || member.Status != (byte)NormalStatus.Normal)
                return DResult.Error("该用户已不是圈内成员！");
            var result = UnitOfWork.Transaction(() =>
            {
                member.Status = (byte)NormalStatus.Delete;
                MemberRepository.Update(m => new { m.Status }, member);
                group.MemberCount -= 1;
                GroupRepository.Update(g => new { g.MemberCount }, group);

                MessageContract.SendMessage(new SystemMessageSendDto(memberId)
                {
                    SenderId = operateId,
                    Title = "你被“{0}”圈主移除".FormatWith(group.GroupName)
                });
            });
            return DResult.FromResult(result);
        }

        #endregion

        #region 圈成员

        /// <summary> 圈成员 </summary>
        /// <param name="groupId"></param>
        /// <param name="role">游民 = 不限</param>
        /// <param name="includeParents">是否包含父母</param>
        /// <param name="page">分页</param>
        /// <returns></returns>
        public DResults<MemberDto> GroupMembers(string groupId, UserRole role = UserRole.Caird,
            bool includeParents = false, DPage page = null)
        {
            if (string.IsNullOrEmpty(groupId))
                return DResult.Errors<MemberDto>("参数错误！");
            if (page == null)
                page = DPage.NewPage(0, 200);
            var groupResult = LoadById(groupId);
            if (!groupResult.Status)
                return DResult.Errors<MemberDto>(groupResult.Message);

            var members = MemberRepository.Where(u => u.GroupId == groupId && u.Status == (byte)NormalStatus.Normal);
            if (role != UserRole.Caird)
            {
                members = members.Where(t => (t.MemberRole & (byte)role) > 0);
            }

            var count = members.Count();
            var list = members
                .Select(u => new { u.MemberId, u.MemberRole, u.AddedAt, u.BusinessCard, u.LastUpDateTime, u.TopicNum })
                .OrderByDescending(u => u.MemberRole)
                .ThenByDescending(u => u.AddedAt)
                .Skip(page.Size * page.Page)
                .Take(page.Size)
                .ToList();
            var memberDtos = new List<MemberDto>();
            if (!list.Any())
                return DResult.Succ(memberDtos, count);
            var ids = list.Select(t => t.MemberId).ToList();
            memberDtos = UserContract.LoadList(ids).OrderByDescending(t => t.Role).MapTo<List<MemberDto>>();
            var parents = new Dictionary<long, List<RelationUserDto>>();
            if (groupResult.Data.Type == (byte)GroupType.Class)
            {
                //班级圈，找家属
                parents = UserContract.ParentsDict(ids);
            }
            memberDtos.ForEach(m =>
            {
                var item = list.FirstOrDefault(t => t.MemberId == m.Id);
                if (item == null)
                    return;
                m.BusinessCard = item.BusinessCard;
                m.AddedTime = item.AddedAt;
                m.LastActive = item.LastUpDateTime ?? item.AddedAt;
                m.TopicCount = item.TopicNum ?? 0;
                if (groupResult.Data.Type == (byte)GroupType.Share)
                {
                    m.Name = string.Empty;
                    if (m.Nick.IsNullOrEmpty())
                    {
                        m.Nick = (m.Email.IsNotNullOrEmpty()
                            ? RegexHelper.EmailNick(m.Email)
                            : RegexHelper.MobileNick(m.Mobile));
                    }
                }
                else if (groupResult.Data.Type == (byte)GroupType.Class)
                {
                    if (m.IsStudent() && parents.ContainsKey(m.Id))
                    {
                        m.Parents = parents[m.Id];
                    }
                }
            });
            return DResult.Succ(memberDtos, count);
        }

        /// <summary>
        /// 圈子成员数量
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="role">游民 = 不限</param>
        /// <returns></returns>
        public int GroupMemberCount(string groupId, UserRole role = UserRole.Caird)
        {
            Expression<Func<TG_Member, bool>> condition = u =>
                u.GroupId == groupId && u.Status == (byte)CheckStatus.Normal;
            var roles = new List<UserRole>
            {
                UserRole.Student,
                UserRole.Teacher,
                UserRole.Parents
            };

            if (roles.Contains(role))
                condition = condition.And(u => u.MemberRole == (byte)role);
            return MemberRepository.Count(condition);
        }

        public Dictionary<string, int> GroupMemberCounts(IEnumerable<string> groupIds, UserRole role = UserRole.Caird)
        {
            Expression<Func<TG_Member, bool>> condition = u =>
               groupIds.Contains(u.GroupId) && u.Status == (byte)CheckStatus.Normal;
            var roles = new List<UserRole>
            {
                UserRole.Student,
                UserRole.Teacher,
                UserRole.Parents
            };

            if (roles.Contains(role))
                condition = condition.And(u => u.MemberRole == (byte)role);
            return MemberRepository.Where(condition).GroupBy(g => g.GroupId)
                .ToDictionary(k => k.Key, v => v.Count());
        }

        public CheckStatus IsGroupMember(long userId, string groupId)
        {
            var inGroup =
                MemberRepository.Exists(
                    m => m.MemberId == userId && m.GroupId == groupId && m.Status == (byte)NormalStatus.Normal);
            if (inGroup)
                return CheckStatus.Normal;
            var waitApply = ApplyRecordRepository.Exists(
                a => a.MemberId == userId && a.GroupId == groupId && a.Status == (byte)CheckStatus.Pending);

            return waitApply ? CheckStatus.Pending : CheckStatus.Invalid;
        }

        #endregion

        #region 退出圈子

        /// <summary>
        /// 退出圈子
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public DResult QuitGroup(long userId, string groupId)
        {
            if (string.IsNullOrEmpty(groupId) || userId <= 0)
                return DResult.Error("参数错误！");

            var member =
                MemberRepository.SingleOrDefault(
                    u => u.MemberId == userId && u.GroupId == groupId && u.Status == (byte)NormalStatus.Normal);

            if (member == null)
                return DResult.Success;

            member.Status = (byte)NormalStatus.Delete;

            //更新圈子的成员数量
            var group = GroupRepository.SingleOrDefault(u => u.Id == member.GroupId);
            if (group == null)
                return DResult.Error("该圈子已经不存在了！");
            if (group.ManagerId == userId)
                return DResult.Error("圈主不能直接退出，请先转让！");
            var user = UserContract.Load(userId);
            if (user == null || (group.GroupType == (byte)GroupType.Class && user.IsStudent()))
                return DResult.Error("您不能主动退出班级圈！");

            group.MemberCount -= 1;

            var result = UnitOfWork.Transaction(() =>
            {
                GroupRepository.Update(g => new { g.MemberCount }, group);
                MemberRepository.Update(m => new { m.Status }, member);
            });
            if (result <= 0)
                return DResult.Error("退出失败，请稍后重试！");
            return DResult.Success;
        }

        public DResults<PendingUserDto> PendingList(string groupId, long userId)
        {
            var group = GroupRepository.Load(groupId);
            if (group == null)
                return DResult.Errors<PendingUserDto>("圈子不存在！");
            if (!IsManager(ParseGroupDto(group), userId))
                return DResult.Errors<PendingUserDto>("只有圈主才能查看申请列表！");
            var pendings = ApplyRecordRepository.Where(
                t => t.GroupId == groupId && t.Status == (byte)CheckStatus.Pending)
                .OrderByDescending(t => t.AddedAt)
                .Select(t => new
                {
                    t.Id,
                    t.MemberId,
                    t.AddedAt,
                    t.Message
                }).ToList();
            var list = UserContract.LoadList(pendings.Select(t => t.MemberId).Distinct()).MapTo<List<PendingUserDto>>();
            foreach (var result in list)
            {
                var item = pendings.FirstOrDefault(t => t.MemberId == result.Id);
                if (item == null) continue;
                result.RecordId = item.Id;
                result.CreationTime = item.AddedAt;
                result.Message = item.Message;
            }
            return DResult.Succ(list, list.Count);
        }

        public void UpdateLastTime(string groupId, long userId)
        {
            if (string.IsNullOrWhiteSpace(groupId) || userId <= 0)
                return;
            var member =
                MemberRepository.FirstOrDefault(
                    m => m.GroupId == groupId && m.MemberId == userId && m.Status == (byte)NormalStatus.Normal);
            if (member == null)
                return;
            member.LastUpDateTime = Clock.Now;
            MemberRepository.Update(m => new { m.LastUpDateTime }, member);
        }

        public DResult JoinGroup(string groupId, long userId)
        {
            if (string.IsNullOrEmpty(groupId) || userId < 1)
                return DResult.Error("参数错误！");
            var status = IsGroupMember(userId, groupId);
            if (status == CheckStatus.Normal)
                return DResult.Error("你已经是圈子成员，无需再次申请！");
            if (status == CheckStatus.Pending)
                return DResult.Error("你的加入申请正在等待审核，请不要重复提交！");
            var user = UserContract.Load(userId);
            if (user == null)
                return DResult.Error("用户信息异常！");
            if (user.Role == (byte)UserRole.Caird)
                return DResult.Error("用户角色异常！");

            var groupInfo =
                GroupRepository.SingleOrDefault(
                    u =>
                        u.Id == groupId && u.Status == (byte)NormalStatus.Normal);
            if (groupInfo == null)
                return DResult.Error("该圈子不存在！");
            if (groupInfo.GroupType != (byte)GroupType.Share)
                return DResult.Error("非分享圈需要申请才能加入！");
            //验证圈子容量
            if (groupInfo.MemberCount + 1 > groupInfo.Capacity)
                return DResult.Error("该圈子已满，不能加入了！");
            var shareGroup = ShareGroupRepository.FirstOrDefault(t => t.Id == groupId);
            if (shareGroup == null || shareGroup.JoinAuth != (byte)GroupJoinAuth.Public)
                return DResult.Error("该分享圈不是公开的，需要申请才能加入！");
            var role = (byte)UserRole.Student;
            if (user.IsTeacher())
                role = (byte)UserRole.Teacher;
            else if (user.IsParents())
                role = (byte)UserRole.Parents;

            var insert = true;
            var memberInfo = MemberRepository.FirstOrDefault(t => t.GroupId == groupId && t.MemberId == userId);
            if (memberInfo == null)
            {
                memberInfo = new TG_Member
                {
                    AddedAt = Clock.Now,
                    BusinessCard = string.Empty,
                    GroupId = groupId,
                    Id = IdHelper.Instance.Guid32,
                    MemberId = userId,
                    Status = (byte)NormalStatus.Normal,
                    MemberRole = role,
                    LastUpDateTime = Clock.Now
                };
            }
            else
            {
                insert = false;
                memberInfo.Status = (byte)NormalStatus.Normal;
            }
            var result = UnitOfWork.Transaction(() =>
            {
                if (insert)
                {
                    MemberRepository.Insert(memberInfo);
                }
                else
                {
                    MemberRepository.Update(m => new { m.Status }, memberInfo);
                }
                groupInfo.MemberCount++;
                GroupRepository.Update(g => new { g.MemberCount }, groupInfo);
            });
            return DResult.FromResult(result);
        }

        /// <summary> 批量添加圈子成员 </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <param name="operatorId"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public DResults<long> AddMembers(string groupId, long[] userIds, long operatorId, string remark = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return DResult.Errors<long>("圈子ID不能为空");
            if (userIds == null || !userIds.Any())
                return DResult.Errors<long>("没有任何待加入的成员");
            if (operatorId <= 0)
                return DResult.Errors<long>("操作人ID不能为空");
            var group = GroupRepository.Load(groupId);
            if (group == null || group.Status == (byte)NormalStatus.Delete)
                return DResult.Errors<long>("圈子不存在或已删除");
            var users = UserContract.LoadList(userIds);
            var records = new List<TG_ApplyRecord>();
            var members = new List<TG_Member>();
            var updateMembers = new List<TG_Member>();
            var idHelper = IdHelper.Instance;
            var errors = new StringBuilder();
            foreach (var user in users)
            {
                var result = AddGroupCheck(user, group, false);
                if (!result.Status)
                {
                    errors.AppendLine($"{user.Name}[No.{user.Code}]:{result.Message}<br/>");
                    continue;
                }
                var record = new TG_ApplyRecord
                {
                    Id = idHelper.Guid32,
                    AddedAt = Clock.Now,
                    AddedBy = operatorId,
                    GroupId = groupId,
                    MemberId = user.Id,
                    CheckedMessage = remark ?? "后台人员添加",
                    CheckedAt = Clock.Now,
                    Status = (byte)CheckStatus.Normal
                };
                records.Add(record);
                var member = MemberRepository.FirstOrDefault(t => t.GroupId == groupId && t.MemberId == user.Id);
                if (member == null)
                {
                    var role = (byte)user.CurrentRole();
                    member = new TG_Member
                    {
                        Id = idHelper.Guid32,
                        AddedAt = Clock.Now,
                        BusinessCard = string.Empty,
                        GroupId = groupId,
                        MemberId = user.Id,
                        Status = (byte)NormalStatus.Normal,
                        MemberRole = role,
                        LastUpDateTime = Clock.Now
                    };
                    members.Add(member);
                }
                else
                {
                    member.Status = (byte)NormalStatus.Normal;
                    updateMembers.Add(member);
                }
                group.MemberCount++;
            }
            if (!members.Any() && !updateMembers.Any())
            {
                return DResult.Errors<long>(errors.ToString());
            }
            var effectResult = UnitOfWork.Transaction(() =>
            {
                if (records.Any())
                    ApplyRecordRepository.Insert(records.ToArray());
                if (members.Any())
                    MemberRepository.Insert(members.ToArray());
                if (updateMembers.Any())
                    MemberRepository.Update(m => new { m.Status }, updateMembers.ToArray());
                GroupRepository.Update(g => new { g.MemberCount }, group);
            });
            if (effectResult <= 0)
                return DResult.Errors<long>("服务器异常");
            var ids = members.Select(t => t.MemberId).Union(updateMembers.Select(t => t.MemberId)).ToList();
            return new DResults<long>(ids, -1)
            {
                Message = errors.ToString()
            };
        }

        #endregion
    }
}
