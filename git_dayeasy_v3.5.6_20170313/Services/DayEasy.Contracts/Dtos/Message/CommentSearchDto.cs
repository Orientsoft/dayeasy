
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Message
{
    public class CommentSearchDto : DPage
    {
        public string SourceId { get; set; }

        public string ParentId { get; set; }

        public CommentOrder Order { get; set; }
    }
}
