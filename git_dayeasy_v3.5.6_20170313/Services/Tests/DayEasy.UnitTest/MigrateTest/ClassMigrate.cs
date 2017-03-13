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
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.MigrateTest
{
    /// <summary> 转移班级到圈子 </summary>
    [TestClass]
    public class ClassMigrate : TestBase
    {
        private readonly ITempContract _tempContract;
        private readonly ITempOldContract _tempOldContract;

        public ClassMigrate()
        {
            _tempContract = Container.Resolve<ITempContract>();
            _tempOldContract = Container.Resolve<ITempOldContract>();
        }

        private string ClassName(byte stage, int grade, string name)
        {
            return string.Concat(new[] { "", "小", "初", "高" }[stage], grade, "级", name);
        }

        [TestMethod]
        public void Main()
        {
            var classList = _tempOldContract.ClassList(DPage.NewPage(0, 500));
            if (!classList.Status)
            {
                Console.WriteLine(classList.Message);
                return;
            }
            var list = new List<GroupDto>();
            foreach (var @class in classList.Data)
            {
                var group = new ClassGroupDto
                {
                    Id = @class.Id,
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
                    group.ManagerId =
                        _tempOldContract.RelationList(@class.Id)
                            .Data.First(t => t.UserRole == (byte)UserRole.Teacher)
                            .UserID;
                }
                list.Add(group);
            }
            var result = _tempContract.CreateGroups(list);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        /// <summary> 圈子成员 </summary>
        [TestMethod]
        public void Members()
        {
            var groups = _tempContract.GroupList();
            if (!groups.Status)
            {
                Console.WriteLine(groups.Message);
                return;
            }
            foreach (var @group in groups.Data)
            {
                var members = new List<TG_Member>();
                var records = new List<TG_ApplyRecord>();
                var relations = _tempOldContract.RelationList(@group.Id);
                if (!relations.Status)
                    continue;
                foreach (var relation in relations.Data)
                {
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
            }
        }

        [TestMethod]
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
            var repository = CurrentIocManager.Resolve<IVersion3Repository<TG_Group>>();
            repository.Update(g => new
            {
                g.GroupAvatar
            }, list);
        }
    }
}
