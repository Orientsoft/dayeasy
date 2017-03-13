using System;

namespace DayEasy.Core.Events.EventData
{
    public interface IEventData
    {
        DateTime EventTime { get; set; }

        object EventSource { get; set; }
    }
}
