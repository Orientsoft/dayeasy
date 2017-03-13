using DayEasy.AsyncMission.Jobs.Jobs;
using DayEasy.Utility.Config;
using Shoy.Backgrounder;
using System;

namespace DayEasy.AsyncMission.Jobs
{
    public static class Setup
    {
        private static readonly JobManager MainJobManager;

        static Setup()
        {
            var jobs = ConfigUtils<JobsConfig>.Instance.Get();
            var interval = (jobs == null || jobs.Interval <= 0) ? 5D : jobs.Interval;
            MainJobManager = new MainJob(TimeSpan.FromSeconds(interval)).CreateManager();
        }

        /// <summary> 开始任务 </summary>
        public static void Start()
        {
            MainJobManager.Start();
        }

        /// <summary> 结束任务 </summary>
        public static void ShutDown()
        {
            MainJobManager.Dispose();
        }
    }
}
