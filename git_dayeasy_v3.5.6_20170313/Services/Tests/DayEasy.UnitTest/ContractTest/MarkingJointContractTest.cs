using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking.Joint;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Marking.Services.Helper;
using DayEasy.Office;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class MarkingJointContractTest : TestBase
    {
        private readonly IMarkingContract _markingContract;

        public MarkingJointContractTest()
        {
            _markingContract = Container.Resolve<IMarkingContract>();
        }

        [TestMethod]
        public void PublishJointTest()
        {
            //var dto = new JointUsageDto
            //{
            //    UserId = 900000000001,
            //    GroupId = "6ba01a94dbbf4994aa8160ae1b612a45",
            //    PaperId = "60be18c556194362a0460d962c44322f",
            //    Dists = new List<JointDistDto>
            //    {
            //        new JointDistDto
            //        {
            //            SectionType = 1,
            //            TeacherIds = new List<long>{900000000001},
            //            QuestionIds = new List<string>
            //            {
            //                "391a70d21fb3405890fb1ee107593477",
            //                "4d999583240246c4a3ccab3802b11288",
            //                "72a4570f82394f458e9a26d2b196657d",
            //                "7e333adc4f38437c98822848b3d4850c",
            //                "8c3aabf32328472aaee679fd43c34802"
            //            }
            //        },
            //        new JointDistDto
            //        {
            //            SectionType = 1,
            //            TeacherIds = new List<long>{922936263650},
            //            QuestionIds = new List<string>
            //            {
            //                "90c86634f22d4fb28809a24af17b99d2",
            //                "a16f71cbfb844e7eaaa080a01afddf9d",
            //                "b6da43ff28374e8da274e4d9dfbc05b0",
            //                "b8d42e9d0e3e4adab6c055ac433498bd",
            //                "e3ce3de560ea474881392984dbac1357"
            //            }
            //        },
            //        new JointDistDto
            //        {
            //            SectionType = 2,
            //            TeacherIds = new List<long>{922936263650},
            //            QuestionIds = new List<string>
            //            {
            //                "372cdcff56284a94a0f2e2c84799f4e3",
            //                "667dbf608d31431ab6c55c7752bae88e",
            //                "796f46a3dfdf42b6bc764254b3cd2075",
            //                "7a13a5fcfd1c44e29494012dd7d4aced"
            //            }
            //        },
            //        new JointDistDto
            //        {
            //            SectionType = 2,
            //            TeacherIds = new List<long>{900000000001},
            //            QuestionIds = new List<string>
            //            {
            //                "185112b847ee45f4ab8005b72e81351b",
            //                "26aa2283b1d249d2b2b914e6c958176f",
            //                "2b0f900871b24b2dbaf7f793828b0f94",
            //                "3400e3c827f84e18971a3014dd22789f"
            //            }
            //        }
            //    }
            //};
            //var result = _markingContract.PublishJoint(dto);
            //Console.Write(result.ToString());
        }


        //[TestMethod]
        //public void GetJointScheduleTest()
        //{
        //    var json = _markingContract.GetJointSchedule("84d321b619be4da19ef694e5c102265f", 922936263650L);
        //    Console.Write(json);
        //}

        //[TestMethod]
        //public void JointMarkingCheckTest()
        //{
        //    var json = _markingContract.JointMarkingCheck("f9629eaba27a4e2eadcf1e36c9af87ed", 2, 900000000001);
        //    Console.Write(json.ToString());
        //}

        [TestMethod]
        public void SetMakringModeTest()
        {
            //const string joint = "bf5fc59c6f424a8cba89d205347eadcf";
            //const long teacherId = 334095189371L;
            //var result = _markingContract.SetMarkingMode(joint, teacherId, JointMarkingMode.Bag);
            //Console.WriteLine(result);
        }

        [TestMethod]
        public void InitPictureTaskTest()
        {
            //            const string joint = "123456";
            //            var pictures = new Dictionary<byte, List<string>>
            //            {
            //                {1, new List<string> {"picture001", "picture002", "picture003", "picture004","picture005","picture006"}},
            //                {2, new List<string> {"picture101", "picture102", "picture103", "picture104"}}
            //            };
            //            var questions = new Dictionary<byte, List<string>>
            //            {
            //                {1, new List<string> {"abc", "bcd", "cde"}},
            //                {2, new List<string> {"123", "234", "345"}}
            //            };
            const string joint = "8cfe18543cab4ace8c120d233802945f";
            var pictures = new Dictionary<byte, List<string>>
            {
                {
                    1, new List<string>
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
                    }
                }
            };
            var paper = CurrentIocManager.Resolve<IPaperContract>().PaperDetailById("ba3fda82f8bc4eb98025b702d4e89540");

            var questions = paper.Data.PaperSections.SelectMany(s => s.Questions)
                .Where(q => !q.Question.IsObjective)
                .GroupBy(q => q.PaperSectionType)
                .ToList()
                .ToDictionary(k => k.Key, v => v.OrderBy(q => q.Sort).Select(q => q.Question.Id).ToList());
            JointHelper.Instance(joint).InitPictureTask(pictures, questions).Wait();
        }

        [TestMethod]
        public void MarkingJointPictureTest()
        {
            //            var arrays = new[] { new[] { "abc", "bcd" }, new[] { "abc" }, new[] { "123", "345" }, new[] { "bcd", "cde" }, new[] { "234" } };
            //            const string joint = "123456";
            //            var tasks = new List<Task>();
            //            for (var i = 0; i < 10; i++)
            //            {
            //                var id = 10001L + i;
            //                var ids = arrays[i % 5];
            //                var task = Task.Factory.StartNew(() =>
            //                {
            //                    int count;
            //                    var helper = JointHelper.Instance(joint);
            //                    var result = helper.MarkingJointPicture(id, ids, out count);
            //                    Console.WriteLine("id:{0},qids:{1},count:{2},picture:{3}", id, ids.ToJson(), count, result);
            //                });
            //                tasks.Add(task);
            //            }
            //            Task.WaitAll(tasks.ToArray());
            const string joint = "8cfe18543cab4ace8c120d233802945f";
            const long teacherId = 900000000001L;
            var ids = new List<string>
            {
                "a33aea0737f045538a9f34d313f7d954",
                "391a70d21fb3405890fb1ee107593477",
                "0c4331cdb4fe465f83686073fd0bdf5a",
                "fb528e69037b45759949ef22e85fc2c5"
            };
            var helper = JointHelper.Instance(joint);
            var result = helper.MarkingJointPicture(teacherId, ids, -1);
            Console.WriteLine("qids:{0},picture:{1}", ids.ToJson(), result.ToJson());
        }

        [TestMethod]
        public void PictureHistoryTest()
        {
            var result = JointHelper.Instance("123456").PictureHistory(10002, "abc");
            WriteJson(result);
        }

        [TestMethod]
        public void MarkingScheduleTest()
        {
            var instance = JointHelper.Instance("123456");
            var result = instance.MarkingSchedule();
            WriteJson(result);
        }

        [TestMethod]
        public void AreaCacheTest()
        {
            const string joint = "8cfe18543cab4ace8c120d233802945f";
            var result = JointHelper.QuestionAreaCache(joint);
            WriteJson(result);
        }

        [TestMethod]
        public void PublishJointV2Test()
        {
            const long teacherId = 304533611023L;
            const string groupId = "208446a4f1f44c3b8fa34593da56477e";
            const string num = "16041826211";
            var result = _markingContract.PublishJoint(teacherId, groupId, num);
            WriteJson(result);
        }

        [TestMethod]
        public void DistributionJointTest()
        {
            var dto = new JDistributionDto
            {
                UserId = 900000000001L,
                JointBatch = "f97815cb7d8349e99b4eb999158aa509",
                Details = new List<DistributionDetailDto>
                {
                    new DistributionDetailDto
                    {
                        SectionType = 1,
                        Questions = new List<string>(),
                        TeacherIds = new List<long>()
                    }
                }
            };
            var result = _markingContract.DistributionJoint(dto);
            WriteJson(result);
        }

        [TestMethod]
        public void DistributeQuestionsTest()
        {
            const string joint = "f97815cb7d8349e99b4eb999158aa509";
            var result = _markingContract.JointAllot(joint);
            WriteJson(result);
        }

        [TestMethod]
        public void JointMissionTest()
        {
            const string joint = "f97815cb7d8349e99b4eb999158aa509";
            const long teacherId = 900000000001L;
            var result = _markingContract.JointMission(joint, teacherId);
            WriteJson(result);
        }

        [TestMethod]
        public void JointCombineTest()
        {
            const string joint = "8cfe18543cab4ace8c120d233802945f";
            var groups = new List<string[]>
            {
                new[]
                {
                    "a33aea0737f045538a9f34d313f7d954", "391a70d21fb3405890fb1ee107593477",
                    "0c4331cdb4fe465f83686073fd0bdf5a", "fb528e69037b45759949ef22e85fc2c5"
                },
                new[] {"f4df949196de46e0976544f1525f225a"}
            };
            const long teacherId = 900000000001L;
            var result = _markingContract.JointCombine(joint, groups, teacherId);
            WriteJson(result);
        }

        [TestMethod]
        public void ChangePicturesTest()
        {
            const string joint = "93bdb2c6c812453ca64133db20b5b93d";
            var groups = new List<string[]>
            {
                new[]
                {
                    "a33aea0737f045538a9f34d313f7d954", "391a70d21fb3405890fb1ee107593477",
                    "0c4331cdb4fe465f83686073fd0bdf5a", "fb528e69037b45759949ef22e85fc2c5"
                }
                //,new[] {"f4df949196de46e0976544f1525f225a"}
            };
            const long teacherId = 900000000001L;
            //            var result = _markingContract.ChangePictures(joint, groups, teacherId, -5);
            //            WriteJson(result);
        }

        [TestMethod]
        public void JointSubmitTest()
        {
            var result = _markingContract.JointSubmit(new JSubmitDto
            {

            });
            WriteJson(result);
        }
        [TestMethod]
        public void InterceptPicture()
        {
            //batch=ae70a28fc48347d590e843e2d3ca292c&paperid=3f52d8476a63458a866e22c53a51995e&type=2&questionid=cb2675f4cca1444aa18d23ece2855567
            const string batch = "ae70a28fc48347d590e843e2d3ca292c";
            const string paperid = "3f52d8476a63458a866e22c53a51995e";
            const MarkingPaperType type = MarkingPaperType.PaperB;
            const long studentNo = 322936262624;
            const string questionid = "cb2675f4cca1444aa18d23ece2855567";
            _markingContract.InterceptPicture(batch, paperid, studentNo, type, questionid);
        }
        [TestMethod]
        public void ObjectiveQuestionScore()
        {
            const string batch = "0a75e6aa930841de9f226ec5dfbf2e66";//AB卷
            const string joinBatch = "c5e1f9af37094c988003107bc0c2a4b0";//非AB卷
            var result = _markingContract.ObjectiveQuestionScore(batch);
            WriteJson(result.Data);
            // Console.ReadLine();
        }

        [TestMethod]
        public void ImportJointDataTest()
        {
            var dt = ExcelHelper.Read("D:\\ddd.xls").Tables[0];
            var dtos = new List<JDataInputDto>();
            foreach (DataRow row in dt.Rows)
            {
                var cols = row.ItemArray;
                if (cols.Length < 3) continue;
                var name = (cols[0] ?? string.Empty).ToString();
                if (string.IsNullOrWhiteSpace(name) || name == "姓名")
                    continue;
                var groupCode = (cols[1] ?? string.Empty).ToString();

                dtos.Add(new JDataInputDto
                {
                    Student = name,
                    GroupCode = groupCode,
                    Scores = cols.Skip(2).Select(t => t.CastTo<decimal>()).ToList()
                });
            }
            //WriteJson(dtos);
            var result = _markingContract.ImportJointData("", dtos);
            WriteJson(result);
        }
    }
}
