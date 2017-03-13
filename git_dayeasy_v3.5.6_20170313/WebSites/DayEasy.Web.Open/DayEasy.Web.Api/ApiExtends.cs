using System;
using DayEasy.Utility.Timing;

namespace DayEasy.Web.Api
{
    public static class ApiExtends
    {
        private static readonly DateTime DefaultTime = new DateTime(2014, 10, 1);

        public static long ToLong(this DateTime time)
        {
            if (time <= DefaultTime) return 0;

            return (time.Ticks - DefaultTime.Ticks) / 10000;
        }

        public static DateTime DateTime(this long ticks)
        {
            if (ticks <= 0) return DefaultTime;
            return new DateTime(ticks * 10000 + DefaultTime.Ticks);
        }

        public static TimeSpan TimeSpan(this long ticks)
        {
            return Clock.Now - ticks.DateTime();
        }
    }
}
