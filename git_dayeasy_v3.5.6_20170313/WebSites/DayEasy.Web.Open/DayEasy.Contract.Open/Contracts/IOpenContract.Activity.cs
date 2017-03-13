
using DayEasy.Contract.Open.Dtos;
using DayEasy.Core.Domain;
using DayEasy.Utility;
using System.Collections.Generic;

namespace DayEasy.Contract.Open.Contracts
{
    /// <summary> 活动相关 </summary>
    public partial interface IOpenContract
    {
        DResult<VTeacherGodDto> MakeTeacherGod(VTeacherGodInputDto dto);

        VTeacherGodDto TeacherGod(string id);

        DResult TeacherMobile(VTeacherMobileInputDto dto);

        List<DKeyValue<int, string>> Areas(int code = 0);
    }
}
