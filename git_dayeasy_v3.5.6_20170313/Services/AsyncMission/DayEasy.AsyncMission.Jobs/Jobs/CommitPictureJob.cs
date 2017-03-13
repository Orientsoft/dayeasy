using DayEasy.AsyncMission.Jobs.JobTasks;
using DayEasy.AsyncMission.Models;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.Threading.Tasks;

namespace DayEasy.AsyncMission.Jobs.Jobs
{
    /// <summary> 提交试卷任务 </summary>
    public class CommitPictureJob : DJob
    {
        private const string JobName = "提交试卷任务";
        private readonly ILogger _logger = LogManager.Logger(JobName);
        public CommitPictureJob(TimeSpan interval, TimeSpan timeout, DateTime? start = null, DateTime? expire = null)
            : base(JobName, interval, timeout, start, expire)
        {
        }

        public CommitPictureJob(TimeSpan interval)
            : base(JobName, interval)
        {
        }

        public override Task Execute()
        {
            return MissionExecute(MissionType.CommitPicture, (mission, log) =>
            {
                var param = JsonHelper.Json<CommitPictureParam>(mission.Params);
                if (param == null)
                    return DResult.Error("异步任务参数异常");
                log(JsonHelper.ToJson(param));
                return new CommitPictureTask(param, log).Execute();
            });
        }
    }
}
