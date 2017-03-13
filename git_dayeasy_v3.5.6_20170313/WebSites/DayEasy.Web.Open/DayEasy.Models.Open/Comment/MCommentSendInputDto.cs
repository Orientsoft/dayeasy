
namespace DayEasy.Models.Open.Comment
{
    public class MCommentSendInputDto:MCommentInputDto
    {
        public string Message { get; set; }
        public string ReplyId { get; set; }
    }
}
