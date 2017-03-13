using System.IO;

namespace DayEasy.Application.Services.Dto
{
    public   class BatchUsersDto
    {
        public string[] users { get; set; }
        public int Role { get; set;}
        public string GroupId { get; set; }
        public Stream bytes { get; set; }
    }
}
