using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.User
{
    /// <summary>
    /// 在校教师
    /// </summary>
   public class BeTeacherDto
    {
        public string Name { get; set; }
        public string SubjectName { get; set; }
        public int SubjectId { get; set; }
        public string UserCode { get; set; }
        public string Mobile { get; set;}
        public long ID { get; set; }
        public DateTime AddedAt { get; set; }
        public string Account { get; set; }
        public string Email { get; set; }
        public byte ValidationType { get; set; }

    }
}
