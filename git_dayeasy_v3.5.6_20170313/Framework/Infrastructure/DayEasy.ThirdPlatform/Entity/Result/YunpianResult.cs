
namespace DayEasy.ThirdPlatform.Entity.Result
{
    public class YunpianResult
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public string Detail { get; set; }
        public SendResult Result { get; set; }
    }
    public class SendResult
    {
        public int Count { get; set; }
        public double Fee { get; set; }
        public long Sid { get; set; }
    }
}
