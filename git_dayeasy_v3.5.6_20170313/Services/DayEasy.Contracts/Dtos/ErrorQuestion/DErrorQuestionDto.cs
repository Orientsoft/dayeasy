using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.ErrorQuestion
{
  public  class DErrorQuestionDto:ErrorQuestionDto
    {
        public string QuestionId { get; set; }
        public int ErrUserCount { get; set; }
        public DateTime CreateTime { get; set; }
        public string QuestionContent { get; set; }
        public int Sort { get; set; }
    }
}
