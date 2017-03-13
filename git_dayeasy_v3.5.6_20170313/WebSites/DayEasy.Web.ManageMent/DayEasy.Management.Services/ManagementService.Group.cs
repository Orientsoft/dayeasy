using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Dependency;
using DayEasy.Office;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public IVersion3Repository<TG_Group, string> GroupRepository { private get; set; }
        public IVersion3Repository<TG_Class, string> ClassRepository { private get; set; }
        public IVersion3Repository<TG_Share, string> ShareRepository { private get; set; }
        public IVersion3Repository<TG_Colleague, string> ColleagueRepository { private get; set; }

        public IVersion3Repository<TG_Member, string> MemberRepository { private get; set; }

        public IVersion3Repository<TS_Agency, string> AgencyRepository { private get; set; }

        public IDayEasyRepository<TS_Area, int> AreaRepository { private get; set; }
        public IGroupContract GroupContract { private get; set; }


        public DResults<TG_Group> GroupSearch(GroupSearchDto searchDto)
        {
            Expression<Func<TG_Group, bool>> condition = g => true;
            if (searchDto.Type >= 0)
                condition = condition.And(g => g.GroupType == searchDto.Type);
            if (searchDto.Level >= 0)
            {
                condition = condition.And(g => g.CertificationLevel.Value == searchDto.Level);
            }
            else if (searchDto.Level == -2)
            {
                condition = condition.And(g => !g.CertificationLevel.HasValue);
            }
            if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                var key = searchDto.Keyword.Trim();
                condition = condition.And(g => g.GroupCode == key || g.GroupName.Contains(key));
            }
            if (!searchDto.ShowDelete)
            {
                condition = condition.And(g => g.Status == (byte)NormalStatus.Normal);
            }
            var groups = GroupRepository.Where(condition);
            if (!string.IsNullOrWhiteSpace(searchDto.AgencyId))
            {
                var classGroups = ClassRepository.Where(c => c.AgencyId == searchDto.AgencyId);
                var colleagueGroups = ColleagueRepository.Where(c => c.AgencyId == searchDto.AgencyId);
                if (searchDto.Type == (byte)GroupType.Class)
                {
                    groups = groups.Join(classGroups, g => g.Id, c => c.Id, (g, c) => g);
                }
                else if (searchDto.Type == (byte)GroupType.Colleague)
                {
                    groups = groups.Join(colleagueGroups, g => g.Id, c => c.Id, (g, c) => g);
                }
                else if (searchDto.Type < 0)
                {
                    var groupIds = classGroups.Select(c => c.Id).Union(colleagueGroups.Select(c => c.Id));
                    groups = groups.Join(groupIds, g => g.Id, c => c, (g, c) => g);
                }
            }

            if (searchDto.ClassType > 0)
            {
                var shareGroups = ShareRepository.Table;

                if (searchDto.ClassType % 100 == 0)//顶级
                {
                    var type = searchDto.ClassType / 100;

                    shareGroups = shareGroups.Where(u => u.ClassType >= type * 100 && u.ClassType < (type + 1) * 100);
                }
                else
                {
                    shareGroups = shareGroups.Where(u => u.ClassType == searchDto.ClassType);
                }

                groups = groups.Where(u => u.GroupType == (byte)GroupType.Share)
                    .Join(shareGroups, g => g.Id, s => s.Id, (g, s) => g);
            }

            var count = groups.Count();
            var list = groups.OrderByDescending(g => g.AddedAt)
                .Skip(searchDto.Page * searchDto.Size)
                .Take(searchDto.Size)
                .ToList();
            return DResult.Succ(list, count);
        }


        public DResults<TG_Group> GroupSearch(List<string> groupIds)
        {
            if (groupIds == null || groupIds.Count < 1)
                return DResult.Errors<TG_Group>("参数错误！");

            var groups = GroupRepository.Where(u => groupIds.Contains(u.Id)).ToList();

            return DResult.Succ(groups, groups.Count);
        }

        public DResult GroupDelete(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return DResult.Error("圈子ID无效！");
            var group = GroupRepository.Load(groupId);
            if (group == null)
                return DResult.Error("圈子不存在！");
            if (group.Status == (byte)NormalStatus.Delete)
            {
                group.Status = (byte)NormalStatus.Normal;
            }
            else
            {
                group.Status = (byte)NormalStatus.Delete;
            }
            var result = GroupRepository.Update(g => new { g.Status }, @group);
            return DResult.FromResult(result);
        }

        public DResult GroupCertificate(string groupId)
        {
            return GroupContract.GroupCertificate(groupId, true);
        }

        public DResult GroupStudentExcel(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return DResult.Error("圈子ID无效！");
            var group = GroupRepository.Load(groupId);
            if (group == null)
                return DResult.Error("圈子不存在！");
            if (group.GroupType != (byte)GroupType.Class)
                return DResult.Error("班级圈才能导出学生二维码信息！");
            var memerIds =
                MemberRepository.Where(
                    m =>
                        m.GroupId == groupId && m.Status == (byte)NormalStatus.Normal &&
                        (m.MemberRole & (byte)UserRole.Student) > 0)
                    .Select(m => m.MemberId).ToList();
            if (!memerIds.Any())
                return DResult.Error("圈内没有学生！~");
            var students =
                UserRepository.Where(u =>
                    u.Status != (byte)UserStatus.Delete
                    && (u.Role & (byte)UserRole.Student) > 0
                    && memerIds.Contains(u.Id))
                    .Select(u => new
                    {
                        id = u.Id,
                        name = u.TrueName,
                        code = u.UserCode,
                        email = u.Email,
                        num = u.StudentNum
                    }).OrderBy(t => t.name).ToList();
            var fileName = string.Format("{0}-学生资料导出-{1}.xls", group.GroupName, DateTime.Now.ToString("yyyyMMdd"));
            //
            var ds = new DataSet();
            var dt = new DataTable("学生资料");
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("ClassId", typeof(string));
            dt.Columns.Add("UserCode", typeof(string));
            dt.Columns.Add("StudentNum", typeof(string));
            dt.Rows.Add("ID", "名称", "帐号", "班级ID", "得一号", "学号");
            ds.Tables.Add(dt);
            students.ForEach(
                u =>
                    dt.Rows.Add(u.id, u.name ?? string.Empty, u.email ?? string.Empty, groupId, u.code,
                        u.num ?? string.Empty));
            ExcelHelper.Export(ds, fileName);
            return DResult.Success;
        }

        public List<TG_Share> ShareGroups(List<string> shareGroupIds)
        {
            if (shareGroupIds == null || shareGroupIds.Count < 1)
                return null;

            return ShareRepository.Where(u => shareGroupIds.Contains(u.Id)).ToList();
        }

        public Dictionary<string, string> GroupTags(ICollection<string> groupIds)
        {
            var dict = new Dictionary<string, string>();
            var list =
                ClassRepository.Where(t => groupIds.Contains(t.Id))
                    .Select(t => new { t.Id, type = 0, value = t.GradeYear })
                    .Union(
                        ColleagueRepository.Where(t => groupIds.Contains(t.Id))
                            .Select(t => new { t.Id, type = 1, value = t.SubjectId }))
                    .Union(
                        ShareRepository.Where(t => groupIds.Contains(t.Id))
                            .Select(t => new { t.Id, type = 2, value = t.ClassType }))
                    .ToList();
            if (list.Any())
            {
                list.ForEach(t =>
                {
                    var tag = string.Empty;
                    switch (t.type)
                    {
                        case 0:
                            tag = "{0}年".FormatWith(t.value);
                            break;
                        case 1:
                            tag = SystemCache.Instance.SubjectName(t.value);
                            break;
                        case 2:
                            tag = Consts.Channel(t.value);
                            break;
                    }
                    dict.Add(t.Id, tag);
                });
            }
            return dict;
        }

        public DResults<UserDto> SearchUsers(string groupId, string keyword)
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(keyword))
                return DResult.Errors<UserDto>("圈子Id及关键字不能为空");
            var users =
                UserRepository.Where(u => u.Status == (byte)NormalStatus.Normal && u.Role != (byte)UserRole.Caird);
            var roles = keyword.Split(':');
            if (roles.Length == 2)
            {
                var role = roles[1].To(0);
                keyword = roles[0];
                users = users.Where(u => (u.Role & role) > 0);
            }
            var words = keyword.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                users = users.Where(u => words.Contains(u.UserCode) || words.Contains(u.TrueName));
            }
            else
            {
                users =
                    users.Where(
                        u =>
                            (u.TrueName != null && u.TrueName.Contains(keyword)) ||
                            (u.Email != null && u.Email.Contains(keyword)) || u.UserCode == keyword);
            }
            //同事圈，当前科目
            var subjectId = ColleagueRepository.Where(t => t.Id == groupId).Select(t => t.SubjectId).FirstOrDefault();
            if (subjectId > 0)
            {
                users = users.Where(u => u.SubjectID == subjectId);
            }
            var isShare = ShareRepository.Exists(t => t.Id == groupId);
            if (!isShare)
            {
                users = users.Where(u => (u.Role & (byte)UserRole.Parents) == 0);
            }
            //排除已有成员
            var memberIds = MemberRepository.Where(
                t =>
                    t.GroupId == groupId &&
                    (t.Status == (byte)CheckStatus.Normal || t.Status == (byte)CheckStatus.Pending))
                .Select(t => t.MemberId).ToList();
            if (memberIds.Any())
            {
                users = users.Where(u => !memberIds.Contains(u.Id));
            }
            //var count = users.Count();
            var dtos = users.Take(100).Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.TrueName,
                Nick = u.NickName,
                Code = u.UserCode,
                Role = u.Role,
                SubjectId = u.SubjectID ?? 0,
                Avatar = u.HeadPhoto
            }).ToList();
            dtos.Foreach(dto =>
            {
                if ((dto.Role & (byte)UserRole.Teacher) > 0 && dto.SubjectId > 0)
                {
                    dto.SubjectName = SystemCache.Instance.SubjectName(dto.SubjectId);
                }
                if (string.IsNullOrWhiteSpace(dto.Avatar))
                    dto.Avatar = Consts.DefaultAvatar();
            });
            return DResult.Succ(dtos, -1);
        }

        public DResult PublishJoint(string paperCode, string groupId, long userId, long operatorId)
        {
            var markingContract = CurrentIocManager.Resolve<IMarkingContract>();
            return markingContract.PublishJoint(userId, groupId, paperCode);
        }

        public DResult UpdateGroup(UpdateGroupInputDto inputDto)
        {
            if (inputDto == null || string.IsNullOrWhiteSpace(inputDto.Id))
                return DResult.Error("参数错误");
            if (string.IsNullOrWhiteSpace(inputDto.Name))
                return DResult.Error("请输入圈子名称");
            var group = GroupRepository.Load(inputDto.Id);
            if (group == null || group.Status == (byte)NormalStatus.Delete)
                return DResult.Error("圈子不存在或已删除");

            group.GroupName = inputDto.Name;
            group.GroupSummary = inputDto.Summary;
            TG_Class classModel = null;
            TG_Colleague colleagueModel = null;
            switch (group.GroupType)
            {
                case (byte)GroupType.Class:
                    classModel = ClassRepository.Load(inputDto.Id);
                    if (classModel == null)
                        return DResult.Error("班级圈不存在");
                    if (string.IsNullOrWhiteSpace(inputDto.AgencyId))
                        return DResult.Error("班级圈所属机构不能为空");
                    if (inputDto.GradeYear <= 0)
                        return DResult.Error("班级圈入学年份不正确");
                    classModel.AgencyId = inputDto.AgencyId;
                    classModel.GradeYear = inputDto.GradeYear;
                    break;
                case (byte)GroupType.Colleague:
                    colleagueModel = ColleagueRepository.Load(inputDto.Id);
                    if (colleagueModel == null)
                        return DResult.Error("同事圈不存在");
                    if (string.IsNullOrWhiteSpace(inputDto.AgencyId))
                        return DResult.Error("同事圈所属机构不能为空");
                    colleagueModel.AgencyId = inputDto.AgencyId;
                    break;
            }
            var result = GroupRepository.UnitOfWork.Transaction(() =>
            {
                GroupRepository.Update(g => new {g.GroupName, g.GroupSummary}, group);
                if (classModel != null)
                    ClassRepository.Update(g => new { g.AgencyId, g.GradeYear }, classModel);
                if (colleagueModel != null)
                    ColleagueRepository.Update(g => new { g.AgencyId }, colleagueModel);
            });
            return DResult.FromResult(result);
        }
    }
}
