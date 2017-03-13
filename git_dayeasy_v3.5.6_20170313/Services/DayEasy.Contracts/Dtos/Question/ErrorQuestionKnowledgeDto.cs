using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Dtos.Question
{
    public class ErrorQuestionKnowledgeDto : KnowledgeDto
    {
        public int ErrCount { get; set; }
    }
}
