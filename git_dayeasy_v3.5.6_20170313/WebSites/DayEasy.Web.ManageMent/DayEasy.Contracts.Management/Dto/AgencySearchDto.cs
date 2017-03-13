using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Management.Dto
{
    public class AgencySearchDto : DPage
    {
        public int Stage { get; set; }
        public int Level { get; set; }
        public string Keyword { get; set; }

        public AgencySearchDto()
        {
            Stage = -1;
            Level = -1;
        }
    }
}
