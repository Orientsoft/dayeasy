using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Core;
using DayEasy.Core.Modules;

namespace DayEasy.Group.Services
{
    [DependsOn(typeof(CoreModule))]
    public class GroupServiceModule : DModule
    {
        public override void Initialize()
        {
            AutoMapperHelper.CreateMapper<GroupDto, ClassGroupDto>();
            AutoMapperHelper.CreateMapper<GroupDto, ColleagueGroupDto>();
            AutoMapperHelper.CreateMapper<GroupDto, ShareGroupDto>();
            AutoMapperHelper.CreateMapper<UserDto, PendingUserDto>();
            AutoMapperHelper.CreateMapper<UserDto, MemberDto>();
            base.Initialize();
        }
    }
}
