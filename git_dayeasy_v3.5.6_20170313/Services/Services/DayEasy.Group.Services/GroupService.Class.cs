

using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Utility;
using System.Collections.Generic;
using System.Linq;

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
            var models =
                MemberRepository.Where(m => m.GroupId == colleagueGroupId && m.Status == (byte)NormalStatus.Normal)
                    .Select(m => m.MemberId);
            var classModels = GroupRepository.Where(g => g.GroupType == (byte)GroupType.Class);
            //班级圈列表
            var classList = MemberRepository.Where(m => m.Status == (byte)NormalStatus.Normal)
                .Join(models, m => m.MemberId, mm => mm, (m, mm) => m.GroupId)
                .Join(classModels, m => m, g => g.Id, (m, g) => g.Id).Distinct().ToList();
            return DResult.Succ(classList, -1);
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

        public DResult<Dictionary<string, JGroupInfoDto>> ColleagueClassDict(string colleagueGroupId)
        {
            if (string.IsNullOrWhiteSpace(colleagueGroupId))
                return DResult.Error<Dictionary<string, JGroupInfoDto>>("同事圈ID不能为空！");
            var colleague = GroupRepository.Load(colleagueGroupId);
            if (colleague == null || colleague.GroupType != (byte)GroupType.Colleague)
                return DResult.Error<Dictionary<string, JGroupInfoDto>>("同事圈不存在！！");
            var models =
                MemberRepository.Where(m => m.GroupId == colleagueGroupId && m.Status == (byte)NormalStatus.Normal)
                    .Select(m => m.MemberId);
            var classModels = GroupRepository.Where(g => g.GroupType == (byte)GroupType.Class);
            var studentModels =
                MemberRepository.Where(
                    m => m.Status == (byte)NormalStatus.Normal && m.MemberRole == (byte)UserRole.Student);
            //班级圈列表
            var classList = MemberRepository.Where(m => m.Status == (byte) NormalStatus.Normal)
                .Join(models, m => m.MemberId, mm => mm, (m, mm) => m.GroupId)
                .Join(classModels, m => m, g => g.Id, (m, g) => new {g.Id, g.GroupCode, g.GroupName})
                .GroupJoin(studentModels, g => g.Id, m => m.GroupId,
                    (g, m) =>
                        new {g.Id, g.GroupCode, g.GroupName, students = m.Select(t => t.MemberId)}).ToList();
            var users = UserContract.LoadDList(classList.SelectMany(t => t.students));
            return DResult.Succ(classList.ToDictionary(k => k.GroupCode, v => new JGroupInfoDto
            {
                Id = v.Id,
                Name = v.GroupName,
                Students = users.Where(u => v.students.Contains(u.Id)).ToList()
            }));
        }
    }
}
