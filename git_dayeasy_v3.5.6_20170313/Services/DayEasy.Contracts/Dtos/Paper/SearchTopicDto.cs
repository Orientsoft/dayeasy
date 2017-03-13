
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary> 试卷选择筛选对象 </summary>
    public class SearchTopicDto : DPage
    {
        public SearchTopicDto()
        {
            SubjectId = -1;
            Grade = -1;
            Stage = -1;
        }

        public int Grade { get; set; }
        public int Stage { get; set; }
        public int Source { get; set; }
        public string Key { get; set; }
        public string Kp { get; set; }
        public long UserId { get; set; }
        public string GroupId { get; set; }
        public int SubjectId { get; set; }
    }
}
