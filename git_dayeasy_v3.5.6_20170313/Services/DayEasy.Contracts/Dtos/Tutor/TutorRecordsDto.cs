using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Tutor
{
    public class TutorRecordsDto : DDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Time { get; set; }
    }
}
