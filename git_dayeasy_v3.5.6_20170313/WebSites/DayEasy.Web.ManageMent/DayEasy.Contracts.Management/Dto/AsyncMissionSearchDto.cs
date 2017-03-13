using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Management.Dto
{
    public class AsyncMissionSearchDto : DPage
    {
        public int Type { get; set; }
        public int Status { get; set; }
        public string Keyword { get; set; }

        public AsyncMissionSearchDto()
        {
            Type = -1;
            Status = -1;
        }
    }
}
