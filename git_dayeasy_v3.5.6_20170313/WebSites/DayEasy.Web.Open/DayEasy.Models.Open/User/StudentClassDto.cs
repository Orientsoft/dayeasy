namespace DayEasy.Models.Open.User
{
    public class StudentClassDto : DDto
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string Agency { get; set; }
    }
}
