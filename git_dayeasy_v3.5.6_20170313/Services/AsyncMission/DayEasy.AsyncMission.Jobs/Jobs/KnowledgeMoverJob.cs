using DayEasy.AsyncMission.Models;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Logging;
using System;
using System.Threading.Tasks;

namespace DayEasy.AsyncMission.Jobs.Jobs
{
    public class KnowledgeMoverJob : DJob
    {
        private const string JobName = "知识点转移任务";
        private readonly ILogger _logger = LogManager.Logger(JobName);
        public KnowledgeMoverJob(TimeSpan interval, TimeSpan timeout, DateTime? start = null, DateTime? expire = null)
            : base(JobName, interval, timeout, start, expire)
        {
        }

        public KnowledgeMoverJob(TimeSpan interval)
            : base(JobName, interval)
        {
        }

        public override Task Execute()
        {
            return MissionExecute(MissionType.ChangeAnswer, (mission,log) =>
            {
                var param = JsonHelper.Json<ChangeAnswerParam>(mission.Params);
                if (param == null)
                    return DResult.Error("异步任务参数异常");
                _logger.Info(JsonHelper.ToJson(param));
                return DResult.Success;
            });
        }
    }
}
