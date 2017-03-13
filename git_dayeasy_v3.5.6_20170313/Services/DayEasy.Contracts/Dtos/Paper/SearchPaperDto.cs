using System.Collections.Generic;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Paper
{
    /// <summary> 搜索试卷传输对象 </summary>
    public class SearchPaperDto : DPage
    {
        public SearchPaperDto()
        {
            SubjectId = -1;
            Stage = -1;
            Grade = -1;
            Status = (byte) PaperStatus.Normal;
            Share = (byte) ShareRange.Self;
        }

        public int Share { get; set; }
        public string GroupId { get; set; }
        public long UserId { get; set; }
        public string Key { get; set; }
        public int SubjectId { get; set; }
        public int Stage { get; set; }
        public int Grade { get; set; }
        public int Status { get; set; }
    }
}
