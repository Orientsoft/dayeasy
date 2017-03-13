using DayEasy.AsyncMission.Models;
using System;
using System.Threading.Tasks;

namespace DayEasy.AsyncMission.Jobs.Jobs
{
    /// <summary> 主任务(用于管理子任务模块) </summary>
    public class MainJob : DJob, IDisposable
    {
        private const string JobName = "主任务";
        //private readonly Dictionary<MissionType, DKeyValue<JobManager, ManagerStatus>> _jobManagers;

        public MainJob(TimeSpan interval, TimeSpan timeout, DateTime? start = null, DateTime? expire = null)
            : base(JobName, interval, timeout, start, expire)
        {
            JobHelper.AddManager(MissionType.CommitPicture,
                new CommitPictureJob(TimeSpan.FromSeconds(MissionType.CommitPicture.Interval())).CreateManager());
            JobHelper.AddManager(MissionType.FinishMarking,
                new FinishMarkingJob(TimeSpan.FromSeconds(MissionType.FinishMarking.Interval())).CreateManager());
            JobHelper.AddManager(MissionType.ChangeAnswer,
                new ChangeAnswerJob(TimeSpan.FromSeconds(MissionType.ChangeAnswer.Interval())).CreateManager());
            JobHelper.AddManager(MissionType.ResetJoint,
                new ResetJointJob(TimeSpan.FromSeconds(MissionType.ResetJoint.Interval())).CreateManager());
        }

        public MainJob(TimeSpan interval)
            : this(interval, TimeSpan.MaxValue)
        {
        }

        public override Task Execute()
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var dict in JobHelper.Managers())
                {
                    if (dict.Value == null || dict.Value.Status != ManagerStatus.Sleep)
                        continue;
                    var existMission = MissionHelper.ExistsMission(dict.Key);
                    if (!existMission)
                        continue;
                    JobHelper.UpdateManager(dict.Key, ManagerStatus.Running);
                }
            });
        }

        public void Dispose()
        {
            foreach (var dict in JobHelper.Managers())
            {
                if (dict.Value != null && dict.Value.Status != ManagerStatus.Dispose)
                {
                    JobHelper.UpdateManager(dict.Key, ManagerStatus.Dispose);
                }
            }
        }
    }
}
