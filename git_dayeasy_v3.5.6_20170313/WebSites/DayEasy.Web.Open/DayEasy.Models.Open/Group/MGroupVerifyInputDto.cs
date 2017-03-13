
namespace DayEasy.Models.Open.Group
{
    public class MGroupVerifyInputDto : DDto
    {
        public string RecordId { get; set; }
        public byte Status { get; set; }
        public string Message { get; set; }
    }
}
