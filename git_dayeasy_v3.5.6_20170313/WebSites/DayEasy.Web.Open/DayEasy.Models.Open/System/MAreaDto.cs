
namespace DayEasy.Models.Open.System
{
    public class MAreaDto : DDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentCode { get; set; }
        public string SimplePinYin { get; set; }
        public int Sort { get; set; }
    }
}
