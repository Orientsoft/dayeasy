
using System;

namespace DayEasy.Models.Open
{
    [Serializable]
    public abstract class DDto
    {
        private static readonly DateTime DefaultTime = new DateTime(2014, 10, 1);

        protected long ToLong(DateTime time)
        {
            if (time <= DefaultTime)
                return 0;

            return (time.Ticks - DefaultTime.Ticks) / 10000;
        }

        protected DateTime ToDateTime(long ticks)
        {
            if (ticks <= 0) return DefaultTime;
            return new DateTime(ticks * 10000 + DefaultTime.Ticks);
        }
    }
}
