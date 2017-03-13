using System.Collections.Generic;
using System.Linq;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Domain.Repositories;
using DayEasy.EntityFramework;
using DayEasy.Group.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility;

namespace DayEasy.Group.Services
{
    public class TempService : Version3Service, ITempContract
    {
        public TempService(IDbContextProvider<Version3DbContext> unitOfWork)
            : base(unitOfWork)
        {
        }

        public IVersion3Repository<TG_Group, string> Groups { private get; set; }

        public IVersion3Repository<TG_Class, string> Classes { private get; set; }

        public IVersion3Repository<TG_Member, string> Members { private get; set; }

        public IVersion3Repository<TG_ApplyRecord, string> Records { private get; set; }
        public IVersion3Repository<TS_Agency, string> Agencies { private get; set; }
        public IVersion3Repository<TM_GroupDynamic, string> GroupDynamicRepository { private get; set; }

        public IGroupContract GroupContract { private get; set; }

        public DResult CreateAgencies(IEnumerable<TS_Agency> agencies)
        {
            var result = Agencies.Insert(agencies);
            return result > 0 ? DResult.Success : DResult.Error("添加失败！");
        }

        public DResult CreateGroups(IEnumerable<GroupDto> groups)
        {
            var codeManager = GroupCodeManager.Instance(GroupContract);
            foreach (var @group in groups)
            {
                var groupItem = GroupContract.LoadById(@group.Id);
                if (groupItem.Status && groupItem.Data != null)
                    continue;
                @group.Code = codeManager.GroupCode((GroupType)@group.Type);
                var item = @group.MapTo<TG_Group>();
                item.Status = (byte)NormalStatus.Normal;
                item.GroupAvatar = GroupAvatarHelper.Avatar();
                Groups.Insert(item);
                switch (@group.Type)
                {
                    case (byte)GroupType.Class:
                        var @class = @group as ClassGroupDto;
                        if (@class == null) continue;
                        Classes.Insert(new TG_Class
                        {
                            Id = @class.Id,
                            AgencyId = @class.AgencyId,
                            Stage = @class.Stage,
                            GradeYear = @class.GradeYear
                        });
                        break;
                    case (byte)GroupType.Colleague:
                        break;
                }
            }
            return DResult.Success;
        }

        public DResult UpdateMembers(IEnumerable<TG_Member> members, IEnumerable<TG_ApplyRecord> records,
            TG_Group @group)
        {
            var result = UnitOfWork.Transaction(() =>
            {
                Members.Insert(members);
                Records.Insert(records);
                Groups.Update(group);
            });
            return result > 0 ? DResult.Success : DResult.Error("添加失败！");
        }

        public DResults<TG_Group> GroupList()
        {
            var list = Groups.Where(g => g.Status == 0).ToList();
            return DResult.Succ(list, Groups.Count());
        }

        public DResult<int> BatchInsertDynamics(List<TM_GroupDynamic> dynamics)
        {
            var list = new List<TM_GroupDynamic>();
            dynamics.ForEach(d =>
            {
                if (!GroupDynamicRepository.Exists(t => t.Id == d.Id))
                    list.Add(d);
            });
            var result = GroupDynamicRepository.Insert(list);
            return result > 0 ? DResult.Succ(result) : DResult.Error<int>("添加失败！");
        }
    }
}
