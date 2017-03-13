using DayEasy.AsyncMission.Jobs.JobTasks;
using DayEasy.AsyncMission.Models;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.Threading.Tasks;

namespace DayEasy.AsyncMission.Jobs.Jobs
{
    /// <summary> 修改答案任务 </summary>
    public class ChangeAnswerJob : DJob
    {
        private const string JobName = "修改答案任务";
        private readonly ILogger _logger = LogManager.Logger(JobName);
        public ChangeAnswerJob(TimeSpan interval, TimeSpan timeout, DateTime? start = null, DateTime? expire = null)
            : base(JobName, interval, timeout, start, expire)
        {
        }

        public ChangeAnswerJob(TimeSpan interval)
            : base(JobName, interval)
        {
        }

        public override Task Execute()
        {
            return MissionExecute(MissionType.ChangeAnswer, (mission, log) =>
            {
                var param = JsonHelper.Json<ChangeAnswerParam>(mission.Params);
                if (param == null)
                    return DResult.Error("异步任务参数异常");
                _logger.Info(JsonHelper.ToJson(param));
                return new ChangeAnswerTask(param, log).Execute();
            });
        }
    }
}
