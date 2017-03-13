using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Management.Dto
{
    public class ManagerSearchDto : DPage
    {
        public string Keyword { get; set; }
    }
}
