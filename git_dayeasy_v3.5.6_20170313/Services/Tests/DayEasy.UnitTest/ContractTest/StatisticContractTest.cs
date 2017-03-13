using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Statistic;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Enum.Statistic;
using DayEasy.UnitTest.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class StatisticContractTest : TestBase
    {
        private readonly IStatisticContract _statisticContract;

        public StatisticContractTest()
        {
            _statisticContract = Container.Resolve<IStatisticContract>();
        }

        [TestMethod]
        public void ErrorTopTenTest()
        {
            var result = _statisticContract.ErrorTopTen(new SearchErrorTopTenDto()
            {
                UserId = 322936262624,
                SubjectId = 5
            });

            Console.Write(result);
        }

        [TestMethod]
        public void GetClassScoresTest()
        {
            var result = _statisticContract.GetClassScores(new SearchClassScoresDto()
            {
                UserId = 922936263650,
                SubjectId = 5,
                //GradeYear = 2013
            });

            Console.Write(result);
        }

        [TestMethod]
        public void StudentRankTest()
        {
            var result = _statisticContract.StudentRank("c6f6f56d6cc74da28d3e8d4408291f67", 5);

            Console.Write(result);
        }

        [TestMethod]
        public void GetStudentScoresTest()
        {
            var result = _statisticContract.GetStudentScores(new SearchStudentRankDto()
            {
                StudentId = 322936262624,
                SubjectId = 5
            });

            Console.Write(result);
        }

        [TestMethod]
        public void TestGetStatisticsRank()
        {
            var json = _statisticContract.GetStatisticsRank("6f5a5eb3ddcf4f318bfcc75399f41c4d",
                "c7714cba3e6c4f2086897aba37bce553");
            Console.Write(json);
        }

        [TestMethod]
        public void TestGetStatisticsAvges()
        {
            var json = _statisticContract.GetStatisticsAvges("1283ee5162c54355b231f54cff68423a",
                "dd20c5749e1b43ae816c4bf90cfbcf27", single: true);
            Console.Write(json);
        }


        [TestMethod]
        public void TestGetKpStatistic()
        {
            var json = _statisticContract.GetKpStatistic(new SearchKpStatisticDataDto()
            {
                GroupId = "c6f6f56d6cc74da28d3e8d4408291f67",
                EndTimeStr = "2017-02-26",
                StartTimeStr = "2017-02-06",
                RegistTime = DateTime.Parse("2014/9/19 18:51:32"),
                Role = UserRole.Teacher,
                SubjectId = 5,
                UserId = 900000000001
            });
            Console.Write(json);
        }

        [TestMethod]
        public void SentSmsTest()
        {
            //var str =
            //    "[922936263639,922936263636,922936263638,923936263644,923936263649,922936263624,922936263634,923936263642,922936263626,922936263633,923936263648,922936263628,922936263632,922936263635,923936263643,322936262626,923936263650,922936263625,923936263651,923936263641,322936262624,322936262625,922936263637]";
            //var ids = str.JsonToObject<List<long>>();
            //var json = _statisticContract.SendScoreSms("e8dd8075c83d411c95725de01fbf9471", "46d1524807d54efaa11231de44c079ad", ids);
            Console.Write("success");
        }

        [TestMethod]
        public void QuestionScoresTest()
        {
            const string batch = "e89f0d5c6d7e4d9ca2663149a5283bfe";
            var result = _statisticContract.QuestionScores(batch);
            WriteJson(result);
        }

        [TestMethod]
        public void GetStatisticsRankTest()
        {
            var result = _statisticContract.GetStatisticsRank("b4283002095242c38824e3f1f0627888", "a2a57dc7c8e24b8c860dc744f517a71b");
            Console.Write(result);
        }

        [TestMethod]
        public void StudentScoreTest()
        {
            const long studentId = 322936262625L;
            const string batch = "24bc41bec87d4cc49ddb4701aede67b9";
            var result = _statisticContract.StudentScore(studentId, batch);
            WriteJson(result);
        }

        [TestMethod]
        public void StudentReportTest()
        {
            const long studentId = 322936262625L;
            const string batch = "24bc41bec87d4cc49ddb4701aede67b9";
            var result = _statisticContract.StudentReport(studentId, batch);
            WriteJson(result);
        }

        [TestMethod]
        public void StudentCompareTest()
        {
            const string batch = "b83c68250ffc494ea69cd9a48812b301";
            const string paperId = "7deb31dd47154aa785f41535302cb589";
            const long studentId = 322936262624L;
            const long compareId = 923936263670L;
            var result = _statisticContract.StudentCompare(studentId, compareId, batch, paperId);
            WriteJson(result);
        }

        [TestMethod]
        public void GraspingsTest()
        {
            const string batch = "a7147e0c13634a5881df285948e19405";
            const string paperId = "3457d8e99c1f41258770c2fa33175cce";
            var result = _statisticContract.Graspings(batch, paperId);
            WriteJson(result);
        }

        [TestMethod]
        public void AgencySurveyTest()
        {
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            var result = _statisticContract.AgencySurvey(agencyId);
            WriteJson(result);
        }

        [TestMethod]
        public void AgencyPortraitTest()
        {
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            var result = _statisticContract.AgencyPortrait(agencyId, TimeArea.Week);
            WriteJson(result);
        }

        [TestMethod]
        public void AgencyExaminationMapTest()
        {
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            var result = _statisticContract.AgencyExaminationMap(agencyId, TimeArea.ThreeMonth);
            WriteJson(result);
        }

        [TestMethod]
        public void AgencyRemedy()
        {
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            var result = _statisticContract.AgencyRemedy(agencyId, TimeArea.SixMonth, true);
            WriteJson(result);
        }
    }
}
