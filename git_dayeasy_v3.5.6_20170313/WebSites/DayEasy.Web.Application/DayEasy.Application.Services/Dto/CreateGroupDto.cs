using DayEasy.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Application.Services.Dto
{
   public class CreateGroupDto:DDto
    {
        public string Name { get; set; }
        public byte Type { get; set; }
        public int SubjectId { get; set; }
        public int GradeYear { get; set; }
        public int Stage { get; set;}
    }
}
