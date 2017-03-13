using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Group.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;

namespace DayEasy.MigrateTools.Migrate
{
    /// <summary> 转移班级到圈子 </summary>
    public class ClassMigrate : MigrateBase
    {
        private readonly ILogger _logger = LogManager.Logger<ClassMigrate>();
        private readonly ITempContract _tempContract;
        private readonly ITempOldContract _tempOldContract;
        private readonly IDayEasyRepository<TU_UserAgencyRelation, string> _relationRepository;
        private readonly IVersion3Repository<TG_Group> _groupRepository;
        private readonly IVersion3Repository<TG_Member> _memberRepository;

        public ClassMigrate()
        {
            _tempContract = Container.Resolve<ITempContract>();
            _tempOldContract = Container.Resolve<ITempOldContract>();
            _relationRepository = Container.Resolve<IDayEasyRepository<TU_UserAgencyRelation, string>>();
            _groupRepository = Container.Resolve<IVersion3Repository<TG_Group>>();
            _memberRepository = Container.Resolve<IVersion3Repository<TG_Member>>();
        }

        private string ClassName(byte stage, int grade, string name)
        {
            return string.Concat(new[] { "", "小", "初", "高" }[stage], grade, "级", name);
        }

        public DResults<TU_UserAgencyRelation> RelationList(string groupId)
        {
            var relations =
                _relationRepository.Where(a => a.ClassID == groupId && a.UserRole != 16)
                    .OrderBy(a => a.ApplyTime)
                    .ToList();
            return DResult.Succ(relations, relations.Count());
        }

        /// <summary> 导入现有班级到班级圈 </summary>
        public void Main()
        {
            var classList = _tempOldContract.ClassList(DPage.NewPage(0, 500));
            if (!classList.Status)
            {
                Console.WriteLine(classList.Message);
                return;
            }
            Console.WriteLine("找到{0}条数据", classList.TotalCount);
            var list = new List<GroupDto>();

            try
            {
                foreach (var @class in classList.Data)
                {
                    var id = @class.Id;
                    if (_groupRepository.Exists(g => g.Id == id))
                        continue;
                    var group = new ClassGroupDto
                    {
                        Id = id,
                        Name = ClassName(@class.Stage, @class.GraduateYear, @class.ClassName),
                        Logo = @class.Logo,
                        Type = (byte)GroupType.Class,
                        Count = 0,
                        Capacity = 200,
                        ManagerId = @class.ClassManagerID ?? 0,
                        CreationTime = @class.CreateTime,
                        Stage = @class.Stage,
                        AgencyId = @class.AgencyID,
                        GradeYear = (@class.GraduateYear - (@class.Stage == 1 ? 6 : 3))
                    };
                    if (group.ManagerId <= 0)
                    {
                        //默认第一个教师做圈主
                        var relations = RelationList(@class.Id);
                        group.ManagerId = 610545404131;
                        if (relations.Status && relations.Data.Any())
                        {
                            var rlist = relations.Data.FirstOrDefault(t => t.UserRole == (byte)UserRole.Teacher);
                            if (rlist != null)
                                group.ManagerId = rlist.UserID;
                        }
                    }
                    list.Add(group);
                    Console.WriteLine("导入班级{0}", group.Name);

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            var result = _tempContract.CreateGroups(list);
            if (result.Status)
            {
                var ids = list.Select(t => t.Id).ToList();
                var groupList = _groupRepository.Where(t => ids.Contains(t.Id));
                foreach (var @group in groupList)
                {
                    GroupMember(@group);
                }
            }
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
            Console.WriteLine("共导入{0}个圈子", list.Count);
        }

        private void GroupMember(TG_Group @group)
        {
            Console.WriteLine("导入班级圈子【{0}】成员", group.GroupName);
            var members = new List<TG_Member>();
            var records = new List<TG_ApplyRecord>();
            var relations = RelationList(@group.Id);
            if (!relations.Status)
                return;
            foreach (var relation in relations.Data)
            {
                if (_memberRepository.Exists(m => m.GroupId == @group.Id && m.MemberId == relation.UserID))
                    continue;
                if (relation.Status == (byte)CheckStatus.Normal)
                {
                    members.Add(new TG_Member
                    {
                        Id = relation.Id,
                        GroupId = @group.Id,
                        MemberId = relation.UserID,
                        MemberRole = relation.UserRole,
                        AddedAt = relation.CheckPassTime ?? relation.ApplyTime,
                        Status = 0
                    });
                    @group.MemberCount++;
                }
                if (relation.Status == (byte)CheckStatus.Pending)
                    @group.UnCheckedCount++;
                var msg = relation.ApplyMsg;
                if (!string.IsNullOrWhiteSpace(msg) && msg.Length > 126)
                    msg = msg.Sub(126);
                records.Add(new TG_ApplyRecord
                {
                    Id = relation.Id,
                    GroupId = @group.Id,
                    MemberId = relation.UserID,
                    AddedAt = relation.ApplyTime,
                    AddedBy = relation.UserID,
                    Message = msg,
                    Status = relation.Status,
                    CheckedMessage = relation.RefuseMsg,
                    CheckedAt = relation.CheckPassTime
                });
            }
            _tempContract.UpdateMembers(members, records, @group);
            Console.WriteLine("共导入{0}名成员", members.Count);
        }

        /// <summary> 圈子成员 </summary>
        public void Members()
        {
            var groups = _tempContract.GroupList();
            if (!groups.Status)
            {
                Console.WriteLine(groups.Message);
                return;
            }
            foreach (var @group in groups.Data.Where(g => g.MemberCount == 0))
            {
                GroupMember(@group);
            }
            Console.WriteLine("导入成员完成");
        }

        /// <summary> 默认圈子logo </summary>
        public void DefaultAvatar()
        {
            //            var avatar = GroupAvatarHelper.Avatar();
            //            Console.WriteLine(avatar);
            var groups = _tempContract.GroupList();
            if (!groups.Status)
                return;
            var list = groups.Data.Where(g => string.IsNullOrEmpty(g.GroupAvatar)).ToArray();
            foreach (var tgGroup in list)
            {
                tgGroup.GroupAvatar = GroupAvatarHelper.Avatar();
            }
            var repository = CurrentIocManager.Resolve<IDayEasyRepository<TG_Group>>();
            repository.Update(g => new
            {
                g.GroupAvatar
            }, list);
        }

        public void GroupManager()
        {
            var groupRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Group>>();
            var memberRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Member>>();
            var groups = groupRepository.Table.ToList();
            var updateGroups = new List<TG_Group>();
            var members = new List<TG_Member>();
            const long managerId = 610545404131;
            foreach (var @group in groups)
            {
                if (@group.ManagerId == 0)
                {
                    @group.ManagerId = managerId;
                    @group.MemberCount++;
                    updateGroups.Add(@group);
                    members.Add(new TG_Member
                    {
                        Id = IdHelper.Instance.Guid32,
                        AddedAt = Clock.Now,
                        GroupId = @group.Id,
                        MemberId = managerId,
                        MemberRole = 4,
                        Status = 0,
                        LastUpDateTime = Clock.Now
                    });
                }
                else
                {
                    var id = @group.Id;
                    var manager = @group.ManagerId;
                    if (memberRepository.Exists(m => m.GroupId == id && m.MemberId == manager))
                        continue;
                    @group.MemberCount++;
                    updateGroups.Add(@group);
                    members.Add(new TG_Member
                    {
                        Id = IdHelper.Instance.Guid32,
                        AddedAt = Clock.Now,
                        GroupId = @group.Id,
                        MemberId = manager,
                        MemberRole = 4,
                        Status = 0,
                        LastUpDateTime = Clock.Now
                    });
                }
            }
            groupRepository.UnitOfWork.Transaction(() =>
            {
                groupRepository.Update(g => new { g.ManagerId, g.MemberCount }, updateGroups.ToArray());
                memberRepository.Insert(members);
            });
            Console.WriteLine("成功！");
        }

        public void GroupManagers()
        {
            var groups = _groupRepository.Where(g => g.Status == 0).Select(t => new {t.Id, t.ManagerId}).ToList();
            var list = new List<TG_Member>();
            foreach (var @group in groups)
            {
                var member =
                    _memberRepository.FirstOrDefault(m => m.GroupId == @group.Id && m.MemberId == @group.ManagerId);
                if (member == null)
                {
                    list.Add(new TG_Member
                    {
                        Id = IdHelper.Instance.Guid32,
                        GroupId = @group.Id,
                        AddedAt = Clock.Now,
                        MemberId = @group.ManagerId,
                        MemberRole = 4,
                        Status = 0,
                        LastUpDateTime = Clock.Now
                    });
                }
                else if (member.Status != 0)
                {
                    member.Status = 0;
                    _memberRepository.Update(m => new {m.Status}, member);
                }
            }
            int count = 0;
            if (list.Any())
            {
                count = _memberRepository.Insert(list);
            }
            Console.WriteLine("成功导入{0}条数据", count);
        }
    }
}
