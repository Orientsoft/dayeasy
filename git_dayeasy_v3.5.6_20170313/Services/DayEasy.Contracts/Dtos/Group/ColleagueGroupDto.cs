


namespace DayEasy.Contracts.Dtos.Group
{
    /// <summary> 备课圈 </summary>
    public class ColleagueGroupDto : GroupDto
    {
        /// <summary> 学段 </summary>
        public byte Stage { get; set; }

//        public string AgencyId { get; set; }
        /// <summary> 科目 </summary>
        public int SubjectId { get; set; }
    }
}
