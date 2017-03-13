using DayEasy.AsyncMission.Models;
using DayEasy.Utility;
using System;

namespace DayEasy.AsyncMission.Jobs.JobTasks
{
    public abstract class DTask<T>
        where T : IMissionParam
    {
        protected readonly Action<string> LogAction;
        protected readonly T Param;

        protected DTask(T param, Action<string> logAction)
        {
            Param = param;
            LogAction = logAction ?? (t => { });
        }

        public abstract DResult Execute();
    }
}
