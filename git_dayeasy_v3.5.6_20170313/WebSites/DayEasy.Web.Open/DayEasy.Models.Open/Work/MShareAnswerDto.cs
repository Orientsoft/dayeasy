

namespace DayEasy.Models.Open.Work
{
    public class MShareAnswerDto : DDto
    {
        public string Id { get; set; }
        public long StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentAvatar { get; set; }
        public int LikeCount { get; set; }
    }
}
