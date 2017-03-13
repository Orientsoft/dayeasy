using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Core;
using DayEasy.Core.Modules;

namespace DayEasy.Management.Services
{
    [DependsOn(typeof(CoreModule))]
    public class ManagementModule:DModule
    {
        public override void Initialize()
        {
            AutoMapperHelper.CreateMapper<UserDto, UserActiveDto>();
            base.Initialize();
        }
    }
}
