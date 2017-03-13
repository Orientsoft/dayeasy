using System;
using DayEasy.Utility.Timing;

namespace DayEasy.Core.Events.EventData
{
    public abstract class EventData : IEventData
    {
        public DateTime EventTime { get; set; }
        public object EventSource { get; set; }

        protected EventData()
        {
            EventTime = Clock.Now;
        }
    }
}
