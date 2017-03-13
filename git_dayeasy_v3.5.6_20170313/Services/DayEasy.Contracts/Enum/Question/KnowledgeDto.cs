using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Contracts.Enum.Question
{
    /// <summary>
    /// 知识点传输对象
    /// </summary>
   public class KnowledgeDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int QuestionCount { get; set; }
    }
}
