using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Utility;

namespace DayEasy.Contracts.Management
{
    public partial interface IManagementContract
    {
        DResults<TB_Topic> GetTopics(TopicSearchDto searchDto);

        DResult UpdateTopicStatus(string topicId, TopicStatus status);
    }
}
