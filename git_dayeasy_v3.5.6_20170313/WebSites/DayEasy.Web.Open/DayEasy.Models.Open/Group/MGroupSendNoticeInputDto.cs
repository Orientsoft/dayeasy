
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Models.Open.Group
{
    public class MGroupSendNoticeInputDto : MGroupInputDto
    {
        public int Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Receivers { get; set; }

        public List<long> ReceiverList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Receivers))
                    return new List<long>();
                return Receivers.Split(',').Select(t => Convert.ToInt64(t)).ToList();
            }
        }
    }
}
