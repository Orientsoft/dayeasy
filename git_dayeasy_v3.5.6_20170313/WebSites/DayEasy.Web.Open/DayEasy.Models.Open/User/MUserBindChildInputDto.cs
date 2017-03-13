
namespace DayEasy.Models.Open.User
{
    public class MUserBindChildInputDto : DDto
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public int RelationType { get; set; }
    }

    public class MUserBindChildPlatformInputDto : DDto
    {
        public string PlatformId { get; set; }
        public int PlatformType { get; set; }
        public int RelationType { get; set; }
    }
}
