using System.Collections.Generic;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 临时契约 </summary>
    public interface ITempContract : IDependency
    {
        DResult CreateAgencies(IEnumerable<TS_Agency> agencies);

        DResult CreateGroups(IEnumerable<GroupDto> groups);

        DResult UpdateMembers(IEnumerable<TG_Member> members, IEnumerable<TG_ApplyRecord> records, TG_Group group);

        DResults<TG_Group> GroupList();

        DResult<int> BatchInsertDynamics(List<TM_GroupDynamic> dynamics);
    }
}
