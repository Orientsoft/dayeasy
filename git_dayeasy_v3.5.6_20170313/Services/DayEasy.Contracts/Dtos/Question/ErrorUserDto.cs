using DayEasy.Contracts.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.Question
{
   public class ErrorUserDto: DUserDto
    {
        public int ErrorCount { get; set; }
    }
}
