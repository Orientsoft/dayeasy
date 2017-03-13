using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class MarkingContractTest : TestBase
    {
        private readonly IMarkingContract _markingContract;

        public MarkingContractTest()
        {
            _markingContract = Container.Resolve<IMarkingContract>();
        }

        [TestMethod]
        public void TestMkPictureList()
        {
            var watch = new Stopwatch();
            watch.Start();
            var json = _markingContract.MkPictureList("193f225b1e4b4175ae210af60f942d82", 2);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.Write(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void TestMkPictureDetail()
        {
            var json = _markingContract.MkPictureDetail("41ce35ad33db4f50be83541d38e35666", 0);
            Console.Write(JsonHelper.ToJson(json, NamingType.UrlCase, true));
        }

        [TestMethod]
        public void TestSubmit()
        {
            var item = new MkSubmitDto
            {
                PictureId = "e3219f57fbff41709a4781958f84de7b",
                UserId = 900000000002,
                PaperType = 0,
                Operation = 10,
                Icons = "[{'x':'115','y':'216','t':'0','w':'0','id':'39d0bea34ae24f42aa715232fe4770f2'},{'x':'256','y':'220','t':'1','w':'1','id':'273c187f96ec431b9e1341c87c93c7c4'},{'x':'167','y':'337','t':'1','w':'5','id':'312cdefa7117402c9d0df84dd8b0a15d'},{'x':'259','y':'576','t':'2','w':'10','id':'c266acd266094151b835a3e7cccf0a35'}]",
                Marks = "",
                Details = new List<MkDetailDto>
                {
                    new MkDetailDto
                    {
                        QuestionId = "273c187f96ec431b9e1341c87c93c7c4",
                        SmallQuestionId = null,
                        IsCorrect = false,
                        Score = 3
                    },
                    new MkDetailDto
                    {
                        QuestionId = "312cdefa7117402c9d0df84dd8b0a15d",
                        SmallQuestionId = null,
                        IsCorrect = false,
                        Score = 5
                    },
                    new MkDetailDto
                    {
                        QuestionId = "c266acd266094151b835a3e7cccf0a35",
                        SmallQuestionId = null,
                        IsCorrect = false,
                        Score = 0
                    }
                }
            };
            var json = _markingContract.Submit(item);
            Console.Write(JsonHelper.ToJson(json, NamingType.UrlCase, true));
        }

        [TestMethod]
        public void TestFinished()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var userId = 900000000001;
            var paperId = "46d1524807d54efaa11231de44c079ad";
            var batchs = new List<string>
            {
                "66ab1a9388984587ac3333bb4a8fc9c3",
                "9c4a18a89860409a9d07e618baaba90f",
                "b1e8aded71ed46128b565b3028db4a0d"
            };

            var json = "OK";
            batchs.ForEach(b =>
            {
                //var result = _markingContract.Finished(b, paperId, MarkingStatus.AllFinished, true, true, userId);
                //if (!result.Status)
                //{
                //    json = JsonHelper.ToJson(result, NamingType.UrlCase, true);
                //}

                //Console.WriteLine("batch:" + b);
            });

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalMilliseconds + "---------------------");

            Console.Write(json);
        }

        [TestMethod]
        public void TestMkPicture()
        {
            var json = _markingContract.LoadPictureDto(
                "8ca4668248f84964a26690285999a7a9",
                "011c9ee2081b437187486894c685376d",
                923936263647);
            Console.Write(json);
        }

        [TestMethod]
        public void TestMkResult()
        {
            var json = _markingContract.LoadResultDto(
                "8ca4668248f84964a26690285999a7a9",
                "011c9ee2081b437187486894c685376d",
                923936263647);
            Console.Write(json);
        }

        [TestMethod]
        public void CompleteTest()
        {
            var dto = new CompleteMarkingInputDto
            {
                Batch = "0f7d40e2f77043cda5e57275f383ad3a",
                IsJoint = true,
                SetIcon = true,
                SetMarks = true
            };
            var result = _markingContract.CompleteMarking(dto);
            WriteJson(result);
            //var json = _markingContract.UpdateScoreStatistics(
            //    "5077d26f26bc406faa99ae115c8aed23",
            //    "8de69727679e4cd6ba5bc817285cc2b2",
            //    900000000002,
            //    new List<StudentRankInfoDto>
            //    {
            //        new StudentRankInfoDto
            //        {
            //            Id="5a7d5fca22e34e00b9603425aa2fb0d1",
            //            SectionAScore = 81,
            //            SectionBScore = 50
            //        }
            //    });
            //Console.Write(json);
        }


        [TestMethod]
        public void TranscopeTest()
        {
            var user = CurrentIocManager.Resolve<IDayEasyRepository<TU_User, long>>();
            var group = CurrentIocManager.Resolve<IVersion3Repository<TG_Group>>();

            try
            {

                DTransaction.Use(() =>
                {

                    var userModel = CurrentIocManager.Resolve<IDayEasyRepository<TU_User, long>>().SingleOrDefault(u => u.Email == "1162131470@qq.com");
                    var groupModel = CurrentIocManager.Resolve<IVersion3Repository<TG_Group>>().SingleOrDefault(u => u.Id == "3556f859e7704a12b1beb84c5ae2e67e");

                    Console.WriteLine(userModel.TrueName);
                    Console.WriteLine(groupModel.GroupName);

                    userModel.TrueName = "ybg123";
                    groupModel.GroupName = "ybg-group";
                });
            }
            catch (Exception ex)
            {
                throw;
            }

            Console.WriteLine("OK");
        }

        //[TestMethod]
        //public void LoadJointTest()
        //{
        //    string batch = "";
        //    var result = _markingContract.LoadJoint(batch);
        //    Console.WriteLine(result);
        //}

        [TestMethod]
        public void PublishJointTest()
        {

        }

        //[TestMethod]
        //public void JointListTest()
        //{
        //    var paperId = "a28b7ec337c244439f8310842ef496f1";
        //    long userId = 304533611023;
        //    var result = _markingContract.JointList(paperId, userId);
        //    Console.WriteLine(result);
        //}

        [TestMethod]
        public void SetRightIconTest()
        {
            //var watch = new Stopwatch();
            //watch.Start();
            //_markingContract.SetRightIconAndScoreMarkTest(
            //    "9f7a8e0a4baf47af80e54a99029eb2ae",
            //    "083894fdf10747bf9dd33f162598b0c9", MarkingStatus.FinishedB);
            //watch.Stop();
            //Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            Console.WriteLine("success");
        }

        [TestMethod]
        public void PictureTest()
        {
            var pictureRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingPicture>>();
            var resultRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingResult>>();
            var detailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            var paperId = "4408324f352e4ee19e21be43911d8ab3";
            var pictureIds = new[]
            {
                "01577c0bb7384aab8a8810394ee0ee94", "0d11560be8f34feeb02ca042a66a1e6e",
                //"ad704e65884d46ca95991f2d8857613a", "d74ca198e12341b88f65dbfd0aa3197f",
                //"97ec0ae5da344a0e9bbe5335213c082d", "d602eda7304a470482cedfd1e045858c"
            };
            //var list = pictureRepository.Where(t => pictureIds.Contains(t.Id))
            //    .GroupJoin(resultRepository.Where(r => r.PaperID == paperId), p => new { p.StudentID, p.BatchNo },
            //        r => new { r.StudentID, BatchNo = r.Batch + "d" }, (p, r) => new { p, r = r.FirstOrDefault() });
            //Console.WriteLine(list.ToString());
            //var pictures = list.ToList().ToDictionary(k => k.p.Id, v => new
            //{
            //    picture = v.p,
            //    result = v.r
            //});

            //Console.WriteLine(JsonHelper.ToJson(pictures, NamingType.CamelCase, true));
            var q = from p in pictureRepository.Table
                    where pictureIds.Contains(p.Id)
                    join r in resultRepository.Table.Where(r => r.PaperID == paperId)
                        on new { p.StudentID, p.BatchNo } equals
                        new { r.StudentID, BatchNo = r.Batch + "d" } into r1
                    from rr in r1.DefaultIfEmpty()
                    select new { p, r = rr };

            Console.WriteLine(q.ToString());
            var pictures = q.ToList().ToDictionary(k => k.p.Id, v => new
            {
                picture = v.p,
                result = v.r
            });
            Console.WriteLine(JsonHelper.ToJson(pictures, NamingType.CamelCase, true));

            //Expression<Func<TP_MarkingResult, bool>> condition = t => t.PaperID == paperId;

            //var pictures = pictureRepository.Where(t => pictureIds.Contains(t.Id))
            //    .Join(resultRepository.Where(condition), s => s.BatchNo,
            //        d => d.Batch, (p, r) => new { p, r })
            //    .Where(t => t.p.StudentID == t.r.StudentID).ToList()
            //    .ToDictionary(k => k.p.Id, v => new { picture = v.p, result = v.r });

            //var markingIds = pictures.Values.Select(t => t.result.Id).Distinct();
            //var detailDict = detailRepository.Where(d => markingIds.Contains(d.MarkingID))
            //    .GroupBy(d => d.MarkingID).ToDictionary(k => k.Key, v => v.ToList());

            //Console.WriteLine(JsonHelper.ToJson(pictures, NamingType.CamelCase,true));
            //Console.WriteLine(JsonHelper.ToJson(detailDict, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void UpdateRightIconAndErrorObjMissionTest()
        {
            const string batch = "fd4a6fc38fdc44ef89130e9edad84b99";
            const string paperId = "7deb31dd47154aa785f41535302cb589";
            //_markingContract.Finished(batch, paperId, MarkingStatus.AllFinished, false, false, 0);
        }

        [TestMethod]
        public void InterceptPictureTest()
        {
            //batch=03849420ee674da583fc06962ebf5415&paperId=6a057433a2b349d9b17fd6c2a9fa61de&type=b&questionId=2c90509759df4d89813052296e80a08b
            const string batch = "03849420ee674da583fc06962ebf5415";
            const string paperId = "6a057433a2b349d9b17fd6c2a9fa61de";
            const string questionId = "2c90509759df4d89813052296e80a08b";
            const string picId = "47defdca150441a3b4d7e8ef7a302042";
            //var result = _markingContract.InterceptPicture(picId, batch, questionId);
            //Console.WriteLine(result);
        }

        [TestMethod]
        public void JointListTest()
        {
            var jointMarkingRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointMarking>>();
            var jointDistributionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointDistribution>>();
            const long userId = 922936263627L;
            var list = (from j in jointMarkingRepository.Where(t => t.Status != (byte)MarkingStatus.AllFinished)
                        join d2 in jointDistributionRepository.Table on j.Id equals d2.JointBatch into d1
                        where (j.AddedBy == userId || d1.Any(t => t.TeacherId == userId))
                        select new
                        {
                            Batch = j.Id,
                            j.PaperId,
                            j.Status,
                            Time = j.AddedAt,
                            j.GroupId,
                            CountA = j.PaperACount,
                            CountB = j.PaperBCount,
                            IsOwner = (j.AddedBy == userId),
                            Alloted = (d1.Any()),
                            IsJoint = true
                        }).Take(5).ToList();
            //jointMarkingRepository.Where(t => t.Status != (byte)MarkingStatus.AllFinished)
            //.GroupJoin(jointDistributionRepository.Table, t => t.Id, d => d.JointBatch,
            //    (j, d) => new { j, d = d.DefaultIfEmpty() })
            ////.Where(t => t.j.AddedBy == userId || (t.d != null && t.d.Any(d => d.TeacherId == userId)))
            //.Select(t => new
            //{
            //    Batch = t.j.Id,
            //    t.j.PaperId,
            //    t.j.Status,
            //    Time = t.j.AddedAt,
            //    t.j.GroupId,
            //    CountA = t.j.PaperACount,
            //    CountB = t.j.PaperBCount,
            //    IsOwner = (t.j.AddedBy == userId),
            //    Alloted = (t.d != null),
            //    IsJoint = true
            //}).Take(5).ToList();
            WriteJson(list);
        }
    }
}
