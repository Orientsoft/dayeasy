
namespace DayEasy.Models.Open.Group
{
    public class MGroupCreateInputDto : DDto
    {
        public int Type { get; set; }
        public int Stage { get; set; }
        public int GradeYear { get; set; }
        public string AgencyId { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string UserName { get; set; }
    }
}
