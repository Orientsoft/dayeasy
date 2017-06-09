
using DayEasy.Models.Open.User;
using DayEasy.Utility;

namespace DayEasy.Contract.Open.Contracts
{
    public partial interface IOpenContract
    {
        DResult BindStudent(long parentId, MUserBindChildInputDto dto);
        DResult BindStudentByPlatfrom(long parentId, MUserBindChildPlatformInputDto dto);

        DResult<StudentDto> Student(string code);

        DResults<StudentClassDto> SearchStudent(string keyword);

        /// <summary> 自动登录 </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        DResult AutoLogin(string account);
    }
}
