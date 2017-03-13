using DayEasy.Contracts.Dtos.Elective;
using DayEasy.Core;
using DayEasy.Core.Domain;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 选修课契约 </summary>
    public interface IElectiveContract : IDependency
    {
        /// <summary> 创建选修课 </summary>
        DResult Create(ElectiveInputDto dto);

        /// <summary> 机构选修课列表 </summary>
        /// <param name="agencyId"></param>
        /// <param name="page"></param>
        DResults<ElectiveBatchDto> List(string agencyId, DPage page = null);

        /// <summary> 开启选修课 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult Start(string batch);

        /// <summary> 关闭选修课 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult Close(string batch);

        /// <summary> 机构选课 </summary>
        /// <param name="agencyId"></param>
        /// <param name="userId"></param>
        /// <returns>选课批次</returns>
        string AgencyCourse(string agencyId, long userId);

        /// <summary> 课程列表 </summary>
        /// <param name="bacth"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        DResult<CourseDto> CourseList(string bacth, long userId = 0);

        /// <summary> 删除选修课 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        DResult Delete(string batch);

        /// <summary> 选课 </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult Course(int id, long studentId);

        /// <summary> 退课 </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        DResult QuitCourse(int id, long studentId);

        /// <summary> 选课详情 </summary>
        /// <param name="batch"></param>
        DResults<ElectiveDetailDto> Details(string batch);
    }
}
