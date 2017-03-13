using DayEasy.Core.Cache;
using DayEasy.Utility.Timing;
using System;

namespace DayEasy.Application.Services.Helper
{
    public class LastMarkingDateHelper
    {
        private const string LastMarkingRegion = "user_last_marking";

        public static void Set(long userId)
        {
            CacheManager.GetCacher(LastMarkingRegion).Set(userId.ToString(), Clock.Now);
        }

        public static DateTime Get(long userId)
        {
            var date = CacheManager.GetCacher(LastMarkingRegion).Get<DateTime>(userId.ToString());
            //if (date == DateTime.MinValue)
            //    date = Clock.Now;
            return date;
        }
    }
}
