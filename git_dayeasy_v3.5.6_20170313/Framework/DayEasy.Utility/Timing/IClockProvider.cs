using System;

namespace DayEasy.Utility.Timing
{
    public interface IClockProvider
    {
        DateTime Now { get; }
        DateTime Normalize(DateTime dateTime);
    }
}
