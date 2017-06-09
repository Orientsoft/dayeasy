using DayEasy.Utility.Timing;
using System;

namespace DayEasy.Web.Api
{
    public static class ApiExtends
    {
        private static readonly DateTime DefaultTime = new DateTime(2014, 10, 1);

        public static DateTime ZoneTime = new DateTime(1970, 1, 1);

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

        public static long ToMillisecondsTimestamp(this DateTime dateTime)
        {

            return (long)(dateTime - ZoneTime).TotalMilliseconds;
        }

        /// <summary>
        /// 时间戳转换成日期
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns></returns>
        public static DateTime FromMillisecondTimestamp(this long timestamp)
        {
            return new DateTime(1970, 1, 1).Add(new TimeSpan(timestamp * System.TimeSpan.TicksPerMillisecond)).ToLocalTime();
        }
    }
}
