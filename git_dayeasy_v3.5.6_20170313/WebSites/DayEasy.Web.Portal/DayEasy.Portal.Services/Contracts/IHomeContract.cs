using DayEasy.Contracts.Dtos.User;
using DayEasy.Core;
using DayEasy.Portal.Services.Dto;
using DayEasy.Utility;

namespace DayEasy.Portal.Services.Contracts
{
    public interface IHomeContract : IDependency
    {
        DResult<VHomeDto> HomeData(UserDto user);

        object GroupSearch(string codes);

        object HotTopicAndGroup();

        object AgencySearch(string keyword, int stage = -1, int count = 6);
    }
}
