
namespace DayEasy.Models.Open.Message
{
    public class EmailDto : DDto
    {
        public string Receiver { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
