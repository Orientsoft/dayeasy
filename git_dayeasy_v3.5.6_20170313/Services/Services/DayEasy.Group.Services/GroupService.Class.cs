

using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;
using DayEasy.Contracts.Dtos.Group;

namespace DayEasy.Group.Services
{
    /// <summary> 班级圈相关业务 </summary>
    public partial class GroupService
    {
        public DResults<string> ColleagueClasses(string colleagueGroupId)
        {
            if (string.IsNullOrWhiteSpace(colleagueGroupId))
                return DResult.Errors<string>("同事圈ID不能为空！");
            var colleague = GroupRepository.Load(colleagueGroupId);
            if (colleague == null || colleague.GroupType != (byte)GroupType.Colleague)
                return DResult.Errors<string>("同事圈不存在！！");
            var teachers =
                MemberRepository.Where(m => m.GroupId == colleagueGroupId && m.Status == (byte)NormalStatus.Normal)
                    .Select(m => m.MemberId).ToList();
            var groups = MemberRepository.Where(
                m => teachers.Contains(m.MemberId) && m.Status == (byte)NormalStatus.Normal)
                .Join(GroupRepository.Where(g => g.GroupType == (byte)GroupType.Class), m => m.GroupId, g => g.Id,
                    (m, g) => g)
                .Select(t => t.Id)
                .ToList();
            if (!groups.Any())
                return DResult.Succ(new List<string>(), 0);
            return DResult.Succ(groups, groups.Count);
        }

        public Dictionary<string, UserDto> SubjectTeachers(ICollection<string> classIds, int subjectId)
        {
            var teachers =
                MemberRepository.Where(t => classIds.Contains(t.GroupId)
                                            && (t.MemberRole & (byte)UserRole.Teacher) > 0
                                            && t.Status == (byte)NormalStatus.Normal)
                    .Select(t => new { t.GroupId, t.MemberId })
                    .ToList()
                    .GroupBy(t => t.GroupId)
                    .ToDictionary(k => k.Key, v => v.Select(t => t.MemberId).ToList());
            var users = teachers.SelectMany(t => t.Value).Distinct().ToList();
            var userList = UserContract.LoadList(users).Where(t => t.SubjectId == subjectId).ToList();
            var dict = new Dictionary<string, UserDto>();
            foreach (var teacher in teachers)
            {
                var item = userList.FirstOrDefault(t => teacher.Value.Contains(t.Id));
                if (item == null)
                    continue;
                dict.Add(teacher.Key, item);
            }
            return dict;
        }
    }
}
