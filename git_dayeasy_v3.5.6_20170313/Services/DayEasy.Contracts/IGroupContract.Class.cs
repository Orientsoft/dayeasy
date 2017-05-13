
using System.Collections.Generic;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Utility;
using DayEasy.Contracts.Dtos.Group;

namespace DayEasy.Contracts
{
    /// <summary> 班级相关接口 </summary>
    public partial interface IGroupContract
    {
        /// <summary> 同事圈所任教的班级圈ID </summary>
        /// <param name="colleagueGroupId"></param>
        /// <returns></returns>
        DResults<string> ColleagueClasses(string colleagueGroupId);

        /// <summary> 同事圈所任教的班级圈 </summary>
        /// <param name="colleagueGroupId"></param>
        /// <returns></returns>
        DResult<Dictionary<string, JGroupInfoDto>> ColleagueClassDict(string colleagueGroupId);

            /// <summary> 科目任课老师 </summary>
        /// <param name="classIds"></param>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        Dictionary<string, UserDto> SubjectTeachers(ICollection<string> classIds, int subjectId);
    }
}
