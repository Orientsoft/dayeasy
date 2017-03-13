


namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary> 班级圈Dto </summary>
    public class ClassGroupDto : GroupDto
    {
        /// <summary> 学段 </summary>
        public byte Stage { get; set; }

        //        public string AgencyId { get; set; }
        /// <summary> 入学年份 </summary>
        public int GradeYear { get; set; }
    }
}
