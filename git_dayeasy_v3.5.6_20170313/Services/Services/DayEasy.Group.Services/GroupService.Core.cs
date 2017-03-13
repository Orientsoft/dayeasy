
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Group.Services
{
    public partial class GroupService
    {
        private const string DefBanner = "/image/default/group/banner/g_banner02.jpg";
        /// <summary> 系统契约接口 </summary>
        public ISystemContract SystemContract { private get; set; }
        public ITopicContract TopicContract { private get; set; }

        private GroupDto LoadGroupFromDb(string groupId)
        {
            var group = GroupRepository.Load(groupId);
            return ParseGroupDto(group);
        }

        private GroupDto LoadGroupFromDbByCode(string code)
        {
            var group = GroupRepository.FirstOrDefault(g => g.GroupCode == code);
            return ParseGroupDto(group);
        }

        private ICollection<GroupDto> LoadGroupsFromDb(Expression<Func<TG_Group, bool>> condition)
        {
            var groups = GroupRepository.Where(condition).ToList();
            return ParseGroupDtos(groups);
        }

        #region 不同类型圈子信息填充

        /// <summary> 转换不同类型圈子信息 </summary>
        /// <param name="groupDtos"></param>
        /// <returns></returns>
        private ICollection<GroupDto> ParseGroupDtos(ICollection<GroupDto> groupDtos)
        {
            if (groupDtos == null || !groupDtos.Any())
                return groupDtos;
            var userIds = groupDtos.Select(t => t.ManagerId).Distinct();
            var users = UserContract.LoadListDictUser(userIds);
            var dict = groupDtos.GroupBy(t => t.Type).ToDictionary(k => k.Key, v => v.Select(t => t.Id).ToList());
            //班级圈
            var classGroups = new List<TG_Class>();
            var teacherCounts = new Dictionary<string, int>();
            if (dict.ContainsKey((byte)GroupType.Class))
            {
                var ids = dict[(byte)GroupType.Class];
                classGroups = ClassGroupRepository.Where(c => ids.Contains(c.Id)).ToList();
                teacherCounts = MemberRepository.Where(
                    t =>
                        ids.Contains(t.GroupId) && t.Status == (byte)CheckStatus.Normal &&
                        (t.MemberRole & (byte)UserRole.Teacher) > 0)
                    .Select(t => t.GroupId)
                    .ToList()
                    .GroupBy(t => t)
                    .ToDictionary(k => k.Key, v => v.Count());
            }

            //同事圈
            var colleagueGroups = new List<TG_Colleague>();
            if (dict.ContainsKey((byte)GroupType.Colleague))
            {
                var ids = dict[(byte)GroupType.Colleague];
                colleagueGroups =
                    ColleagueGroupRepository.Where(c => ids.Contains(c.Id)).ToList();
            }

            //分享圈
            var shareGroups = new List<TG_Share>();
            if (dict.ContainsKey((byte)GroupType.Share))
            {
                var ids = dict[(byte)GroupType.Share];
                shareGroups = ShareGroupRepository.Where(s => ids.Contains(s.Id)).ToList();
            }

            //机构
            var agencyIds = classGroups.Select(c => c.AgencyId).Union(colleagueGroups.Select(t => t.AgencyId));
            var agenice = SystemContract.AgencyList(agencyIds);

            var list = new List<GroupDto>();
            foreach (var dto in groupDtos)
            {
                if (dto.GroupBanner.IsNullOrEmpty())
                    dto.GroupBanner = Consts.Config.FileSite + DefBanner;
                var manager = users.ContainsKey(dto.ManagerId) ? users[dto.ManagerId] : new DUserDto();
                dto.Owner = manager.Name;
                switch (dto.Type)
                {
                    case (byte)GroupType.Class:
                        //班级圈
                        var classGroup = dto.MapTo<ClassGroupDto>();
                        var group = classGroups.FirstOrDefault(c => c.Id == dto.Id);
                        if (group != null)
                        {
                            classGroup.AgencyId = group.AgencyId;
                            classGroup.GradeYear = group.GradeYear;
                            classGroup.Stage = group.Stage;
                            classGroup.AgencyName = agenice[group.AgencyId];
                            if (teacherCounts.ContainsKey(group.Id))
                                classGroup.TeacherCount = teacherCounts[group.Id];
                        }
                        list.Add(classGroup);
                        break;
                    case (byte)GroupType.Colleague:
                        //同事圈
                        var colleagueGroup = dto.MapTo<ColleagueGroupDto>();
                        var cGroup = colleagueGroups.FirstOrDefault(c => c.Id == dto.Id);
                        if (cGroup != null)
                        {
                            colleagueGroup.AgencyId = cGroup.AgencyId;
                            colleagueGroup.SubjectId = cGroup.SubjectId;
                            colleagueGroup.Stage = cGroup.Stage;
                            colleagueGroup.AgencyName = agenice[cGroup.AgencyId];
                        }
                        list.Add(colleagueGroup);
                        break;
                    case (byte)GroupType.Share:
                        //分享圈
                        var shareGroup = dto.MapTo<ShareGroupDto>();
                        shareGroup.Owner = manager.Nick;
                        var sGroup = shareGroups.FirstOrDefault(s => s.Id == dto.Id);
                        if (sGroup != null)
                        {
                            shareGroup.ChannelId = sGroup.ClassType;
                            shareGroup.Tags = sGroup.Tags;
                            shareGroup.PostAuth = sGroup.PostAuth;
                            shareGroup.JoinAuth = sGroup.JoinAuth;
                            shareGroup.TopicNum = sGroup.TopicNum;
                            shareGroup.Notice = sGroup.Notice;
                        }
                        list.Add(shareGroup);
                        break;
                }
            }
            return list;
        }

        private ICollection<GroupDto> ParseGroupDtos(ICollection<TG_Group> groups)
        {
            if (groups == null || !groups.Any())
                return new List<GroupDto>();
            var list = groups.MapTo<List<GroupDto>>();
            return ParseGroupDtos(list);
        }

        private GroupDto ParseGroupDto(TG_Group group)
        {
            if (group == null)
                return null;
            var dto = group.MapTo<GroupDto>();
            var list = ParseGroupDtos(new[] { dto }).ToList();
            return list.Any() ? list.First() : dto;
        }
        #endregion

        /// <summary> 用户圈内更新动态数 </summary>
        /// <param name="groupDtos"></param>
        /// <param name="userId"></param>
        private void UserGroupMessageCount(List<GroupDto> groupDtos, long userId)
        {
            if (groupDtos == null || !groupDtos.Any() || userId <= 0)
                return;
            var lastTimeList =
                MemberRepository.Where(
                    m => m.MemberId == userId && m.Status == (byte)NormalStatus.Normal)
                    .Join(GroupRepository.Table, m => m.GroupId, g => g.Id, (m, g) => new
                    {
                        m.GroupId,
                        g.GroupType,
                        m.LastUpDateTime
                    }).ToList();
            if (!lastTimeList.Any())
                return;
            var user = UserContract.Load(userId);
            var dynamicDict = new Dictionary<string, int>();
            var topicDict = new Dictionary<string, int>();
            Dictionary<string, DateTime?> lastTime;
            if (lastTimeList.Any(t => t.GroupType != (byte)GroupType.Share))
            {
                lastTime = lastTimeList.Where(
                    t => t.GroupType == (byte)GroupType.Class || t.GroupType == (byte)GroupType.Colleague)
                    .ToDictionary(k => k.GroupId, v => v.LastUpDateTime);
                dynamicDict = MessageContract.DynamicCountDict(userId, user.Role, lastTime);
            }
            if (lastTimeList.Any(t => t.GroupType == (byte)GroupType.Share))
            {
                lastTime = lastTimeList.Where(t => t.GroupType == (byte)GroupType.Share)
                    .ToDictionary(k => k.GroupId, v => v.LastUpDateTime);
                topicDict = TopicContract.UpdateCountDict(lastTime);
            }
            foreach (var dto in groupDtos)
            {
                if (dto.Type == (byte)GroupType.Share)
                {
                    if (topicDict.ContainsKey(dto.Id))
                        dto.MessageCount = topicDict[dto.Id];
                }
                else
                {
                    if (dynamicDict.ContainsKey(dto.Id))
                        dto.MessageCount = dynamicDict[dto.Id];
                }
            }
        }

        /// <summary> 加入圈子验证 </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <param name="isApply">是否是申请</param>
        /// <returns></returns>
        private DResult AddGroupCheck(UserDto user, TG_Group group, bool isApply = true)
        {
            if (user == null || group == null)
                return DResult.Error("圈子不存在");
            if (group.GroupType != (byte)GroupType.Share && user.IsParents())
                return DResult.Error("家长不能加入班级圈或同事圈");
            switch (group.GroupType)
            {
                case (int)GroupType.Class:
                    if (user.IsTeacher())
                    {
                        //相同科目判断
                        var teachers =
                            MemberRepository.Where(
                                u =>
                                    u.GroupId == group.Id && u.MemberRole == (byte)UserRole.Teacher &&
                                    u.Status == (byte)NormalStatus.Normal).Select(u => u.MemberId).ToList();

                        var userList = UserContract.LoadList(teachers);
                        if (userList != null && userList.Any())
                        {
                            if (userList.Select(u => u.SubjectId).Contains(user.SubjectId))
                                return DResult.Error("已存在相同科目的教师");
                        }
                    }
                    else if (user.IsStudent())
                    {
                        //学生重名验证
                        var students =
                            MemberRepository.Where(
                                u =>
                                    u.GroupId == group.Id && u.MemberRole == (byte)UserRole.Student &&
                                    u.Status == (byte)NormalStatus.Normal).Select(u => u.MemberId).ToList();
                        if (students.Any())
                        {
                            var repeatUser =
                                UserRepository.Where(u => students.Contains(u.Id) && u.TrueName == user.Name)
                                    .Select(u => new { u.ValidationType, u.Email, u.Mobile, u.NickName, u.UserCode })
                                    .FirstOrDefault();
                            if (repeatUser != null)
                            {
                                var msg = StudentRepeatMessage(user.Name, repeatUser.ValidationType,
                                    repeatUser.Email, repeatUser.Mobile, repeatUser.NickName, repeatUser.UserCode);
                                return DResult.Error(msg);
                            }
                        }
                        if (isApply)
                        {
                            var pendings =
                                ApplyRecordRepository.Where(
                                    a => a.GroupId == group.Id && a.Status == (byte)CheckStatus.Pending)
                                    .Select(m => m.MemberId).ToList();
                            if (pendings.Any())
                            {
                                var repeatUser =
                                    UserRepository.Where(u => pendings.Contains(u.Id) && u.TrueName == user.Name)
                                        .Select(u => new { u.ValidationType, u.Email, u.Mobile, u.NickName, u.UserCode })
                                        .FirstOrDefault();
                                if (repeatUser != null)
                                {
                                    var msg = StudentRepeatMessage(user.Name, repeatUser.ValidationType,
                                        repeatUser.Email, repeatUser.Mobile, repeatUser.NickName, repeatUser.UserCode,
                                        true);
                                    return DResult.Error(msg);
                                }
                            }
                        }
                        //一个学段不能同时加入两个班级圈
                        //var classGroup = ClassGroupRepository.Load(group.Id);
                        //if (classGroup == null)
                        //    return DResult.Error("圈子信息异常!");
                        //var classList = ClassGroupRepository.Where(c => c.Stage == classGroup.Stage);
                        //var classCheck = MemberRepository.Where(
                        //    t =>
                        //        t.MemberId == user.Id &&
                        //        (t.Status == (byte)CheckStatus.Normal || t.Status == (byte)CheckStatus.Pending))
                        //    .Join(classList, m => m.GroupId, c => c.Id, (m, c) => new
                        //    {
                        //        m.Id
                        //    }).ToList();
                        //if (classCheck.Any())
                        //    return DResult.Error("同一个学段不能同时加入多个班级圈");
                    }
                    break;
                case (int)GroupType.Colleague:
                    if (user.IsStudent())
                        return DResult.Error("学生不能加入同事圈");
                    var colleague = ColleagueGroupRepository.Load(group.Id);
                    if (colleague == null)
                        return DResult.Error("圈子信息异常");
                    //同事圈科目判断
                    if (colleague.SubjectId != user.SubjectId)
                        return DResult.Error("同事圈科目不一致");
                    break;
                case (byte)GroupType.Share:
                    //所有人都能加
                    break;
            }
            return DResult.Success;
        }

        private static string StudentRepeatMessage(string name, byte validation, string email, string mobile, string nick,
            string code,
            bool isPending = false)
        {
            const string message = "{0}已经存在名字为[{1}]的学生，你可以使用{2}直接登录";
            string account;
            if (!string.IsNullOrWhiteSpace(email))
                account = "邮箱[{0}]".FormatWith(email);
            else if ((validation & (byte)ValidationType.Mobile) > 0)
                account = "手机号码[{0}]".FormatWith(mobile);
            else if ((validation & (byte)ValidationType.Third) > 0)
                account = "昵称为[{0}]的QQ".FormatWith(nick);
            else
                account = "得一号[{0}]".FormatWith(code);
            return message.FormatWith(isPending ? "审核列表中" : "圈子中", name, account);
        }
    }
}
