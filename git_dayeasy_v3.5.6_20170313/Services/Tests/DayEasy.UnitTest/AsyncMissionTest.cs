using DayEasy.AsyncMission;
using DayEasy.AsyncMission.Jobs.JobTasks;
using DayEasy.AsyncMission.Models;
using DayEasy.Contracts;
using DayEasy.Core.Dependency;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DayEasy.UnitTest
{
    [TestClass]
    public class AsyncMissionTest : TestBase
    {
        [TestMethod]
        public void PubMissionTest()
        {
            MissionHelper.PushMission(MissionType.FinishMarking,
                new FinishMarkingParam
                {
                    Batch = "7be45835be3345f9ae4f6e516e8295a5",
                    IsJoint = true
                }, 335603683145, priority: 10);
            //var jobs = new JobsConfig
            //{
            //    Interval = 5,
            //    Jobs = new List<JobConfig>
            //    {
            //        new JobConfig {Type = MissionType.ChangeAnswer,Interval = 20},
            //        new JobConfig {Type = MissionType.CommitPicture,Interval = 30},
            //        new JobConfig {Type = MissionType.FinishMarking,Interval = 30},
            //        new JobConfig {Type = MissionType.ResetJoint,Interval = 30}
            //    }
            //};
            //ConfigUtils<JobsConfig>.Instance.Set(jobs);
        }

        [TestMethod]
        public void CommitPictureAsyncTest()
        {
            const string paperId = "ba3fda82f8bc4eb98025b702d4e89540";
            var ids = new List<string>
            {
                "18c2537c3fe94dc2b158caee7d56cf04",
                "49fc20f83f7a46a8a435a66139b73cf2",
                "4c4770d204c14e46a147c6c5a2ff8fdb",
                "4e02d6814d9448b68b4187691e423e9a",
                "60b9eca39c4b4d4e9b54cf37ca2a5bc2",
                "889bf9cc21894cf78f8417b131e92e50",
                "d061e59eebc24d25bd53c57f967ee95e",
                "eaf909f340f243f2a1725ac5a88c13fa",
                "f0f7fcde1038445099a7305e9c2a9007",
                "fb2ab8a4b6084dbc93883c0cf28090ad"
            };
            const string joint = "8cfe18543cab4ace8c120d233802945f";
            MissionHelper.PushMission(MissionType.CommitPicture, new CommitPictureParam
            {
                PaperId = paperId,
                PictureIds = ids,
                JointBatch = joint
            });
        }

        [TestMethod]
        public void ExistsTest()
        {
            var exists = MissionHelper.ExistsMission(MissionType.FinishMarking);
            Console.WriteLine(exists);
        }

        [TestMethod]
        public void MissionTest()
        {
            var mission = MissionHelper.Get(MissionType.FinishMarking);
            mission?.Fail("失效测试", retry: false);
            mission = MissionHelper.Get(MissionType.FinishMarking);
            mission?.Finish();
        }

        [TestMethod]
        public void MissionListTest()
        {
            var list = MissionHelper.Missions();
            Console.WriteLine(JsonHelper.ToJson(list, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void FinishedTest()
        {
            var param = new FinishMarkingParam
            {
                Batch = "7af3f9538164486fb2a8f6805a390364",
                IsJoint = true,
                SetIcon = true,
                SetMarks = true
            };
            var result = new FinishMarkingTask(param, Console.WriteLine).Execute();
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void KpStatisticTest()
        {
            var paperContract = CurrentIocManager.Resolve<IPaperContract>();
            var paper = paperContract.PaperDetailById("b6888616762e480c9ba1f6969909efea").Data;
            var dtos = FinishMarkingTask.InitKpStatistics(paper, new Dictionary<string, string> { { "e9d75c37aa7d4370a186fcdaf1d923b1", "f8fcef6d7a9d4833bf0293d2346750b4" } });
            WriteJson(dtos);
        }
    }
}
