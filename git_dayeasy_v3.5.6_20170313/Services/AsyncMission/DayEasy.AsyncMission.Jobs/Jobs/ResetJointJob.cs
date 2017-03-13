using DayEasy.AsyncMission.Jobs.JobTasks;
using DayEasy.AsyncMission.Models;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using System;
using System.Threading.Tasks;

namespace DayEasy.AsyncMission.Jobs.Jobs
{
    /// <summary> 重置协同阅卷任务 </summary>
    public class ResetJointJob : DJob
    {
        private const string JobName = "重置协同阅卷任务";
        public ResetJointJob(TimeSpan interval, TimeSpan timeout, DateTime? start = null, DateTime? expire = null)
            : base(JobName, interval, timeout, start, expire)
        {
        }

        public ResetJointJob(TimeSpan interval)
            : base(JobName, interval)
        {
        }

        public override Task Execute()
        {
            return MissionExecute(MissionType.ResetJoint, (mission, log) =>
            {
                var param = JsonHelper.Json<ResetJointParam>(mission.Params);
                if (param == null)
                    return DResult.Error("异步任务参数异常");
                log(JsonHelper.ToJson(param));
                return new ResetJointTask(param, log).Execute();
            });
        }
    }
}
