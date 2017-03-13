using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Domain;

namespace DayEasy.Contracts.Dtos.Topic
{
    public class SearchTopicDto : DPage
    {
        public SearchTopicDto()
        {
            ClassType = -1;
            State = (byte) TopicState.Normal;
            Order = TopicOrder.TimeDesc;
        }

        public string GroupId { get; set; }
        public int ClassType { get; set; }
        public int State { get; set; }
        public List<string> Tags { get; set; } 
        public TopicOrder Order { get; set; }
        public List<string> ExceptIds { get; set; } 
    }
}
