
namespace DayEasy.Models.Open.Message
{
    public class SmsDto : DDto
    {
        /// <summary> Mobile </summary>
        public string Mobile { get; set; }
        public bool Check { get; set; }

        public SmsDto()
        {
            Check = true;
        }
    }
}
