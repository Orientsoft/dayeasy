
using System.Collections.Generic;

namespace DayEasy.Models.Open.User
{
    public class StudentDto : DDto
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public Dictionary<string, string> ClassList { get; set; }
    }
}
