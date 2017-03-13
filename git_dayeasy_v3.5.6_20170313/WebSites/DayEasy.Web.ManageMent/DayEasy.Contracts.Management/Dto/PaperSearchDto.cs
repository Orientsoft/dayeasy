using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Management.Dto
{
    public class PaperSearchDto : DPage
    {
        public int Subject { get; set; }
        public string Keyword { get; set; }
        public int ShareRange { get; set; }
        public int Status { get; set; }

        public PaperSearchDto()
        {
            ShareRange = -1;
            Status = -1;
        }
    }
}
