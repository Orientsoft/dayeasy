using DayEasy.AsyncMission.Models;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Logging;
using Shoy.Backgrounder;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.AsyncMission.Jobs
{
    public abstract class DJob : Job
    {
        protected ILogger Logger = LogManager.Logger(typeof(DJob));
        protected DJob(string name, TimeSpan interval, TimeSpan timeout, DateTime? start = null, DateTime? expire = null)
            : base(name, interval, timeout, start, expire)
        {
        }

        protected DJob(string name, TimeSpan interval)
            : base(name, interval)
        {
        }

        protected Task MissionExecute(MissionType type, Func<MAsyncMission, Action<string>, DResult> missionAction)
        {
            return Task.Factory.StartNew(() =>
            {
                var mission = MissionHelper.Get(type);
                if (mission == null)
                {
                    //没有任务时，停止当前任务
                    JobHelper.UpdateManager(type, ManagerStatus.Sleep);
                    return;
                }
                var sb = new StringBuilder();
                Action<string> logAction = t =>
                {
                    sb.AppendLine($"{Utils.GetTimeNow()} - {t}");
                    //sb.AppendLine();
                };
                var failMsg = mission.FailCount > 0 ? $"({mission.FailCount + 1})" : string.Empty;
                logAction($"开始任务：{type.GetText()}{failMsg}");

                var watcher = new Stopwatch();
                watcher.Start();
                try
                {
                    var result = missionAction(mission, logAction);
                    if (result.Status)
                    {
                        mission.Finish();
                    }
                    else
                    {
                        mission.Fail(result.Message, retry: false);
                        Logger.Info($"任务失败信息：{result.Message}");
                        logAction($"任务失败信息：{result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    mission.Fail(ex: ex);
                    Logger.Error(ex.Message, ex);
                    logAction($"异常:{ex.Message}");
                }
                finally
                {
                    watcher.Stop();
                    logAction($"任务总耗时:{watcher.ElapsedMilliseconds}ms");
                    mission.AppendLog(sb.ToString());
                    //Logger.Info(sb.ToString());
                }
            });
        }
    }
}
