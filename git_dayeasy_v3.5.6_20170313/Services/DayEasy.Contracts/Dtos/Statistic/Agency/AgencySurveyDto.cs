using DayEasy.Core.Domain;
using DayEasy.Core.Domain.Entities;
using System.Collections.Generic;

namespace DayEasy.Contracts.Dtos.Statistic.Agency
{
    /// <summary> 学校概况 </summary>
    public class AgencySurveyDto : RefreshDto
    {
        /// <summary> 学生人数 </summary>
        public int StudentCount { get; set; }
        /// <summary> 教师人数 </summary>
        public int TeacherCount { get; set; }
        /// <summary> 班级数 </summary>
        public int ClassCount { get; set; }
        /// <summary> 同事圈数 </summary>
        public int ColleagueCount { get; set; }
        /// <summary> 主页访问数 </summary>
        public int VisitCount { get; set; }
        /// <summary> 目标学校人数 </summary>
        public int TargetCount { get; set; }
        /// <summary> 班级圈概况 </summary>
        public List<AgencyGroupServeyDto> ClassList { get; set; }
        /// <summary> 同事圈概况 </summary>
        public List<AgencyGroupServeyDto> ColleagueList { get; set; }
    }

    /// <summary> 圈子概况 </summary>
    public class AgencyGroupServeyDto : DDto
    {
        /// <summary> Id </summary>
        public int Id { get; set; }
        /// <summary> 键，班级圈:级,同事圈:科目 </summary>
        public string Key { get; set; }
        /// <summary> 用户分布 </summary>
        public List<DKeyValue<string, int>> Users { get; set; }
    }
}
