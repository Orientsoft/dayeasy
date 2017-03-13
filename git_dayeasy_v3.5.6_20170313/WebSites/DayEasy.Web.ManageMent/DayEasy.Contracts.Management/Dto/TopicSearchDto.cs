using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Management.Dto
{
    public class TopicSearchDto : DPage
    {
        public int Auth { get; set; }
        public int ClassType { get; set; }
        public int TopicStatus { get; set; }
        public string Sort { get; set; }
        public string KeyWord { get; set; }
    }
}
