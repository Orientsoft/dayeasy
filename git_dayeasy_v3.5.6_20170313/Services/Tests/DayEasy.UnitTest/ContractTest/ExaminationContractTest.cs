using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Examination;
using DayEasy.Contracts.Enum;
using DayEasy.Examination.Services.Helper;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class ExaminationContractTest : TestBase
    {
        private readonly IExaminationContract _examinationContract;

        public ExaminationContractTest()
        {
            _examinationContract = Container.Resolve<IExaminationContract>();
        }

        [TestMethod]
        public void SendExamTaskTest()
        {
            const string examId = "81ec269cd201481b98c749504bc7fe8b";
            ExaminationTask.Instance.GenerateExaminatiomRanksAsync(examId).Wait();
        }

        [TestMethod]
        public void JointListTest()
        {
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            var result = _examinationContract.JointList(new JointSearchDto
            {
                AgencyId = agencyId,
                Subject = 1,
                Status = JointStatus.Normal,
                Page = 0,
                Size = 30
            });
            //agencyId, page: DPage.NewPage(0, 30)
            Console.WriteLine(result);
        }

        [TestMethod]
        public void RankingsTest()
        {
            const string examId = "81ec269cd201481b98c749504bc7fe8b";
            var result = _examinationContract.Rankings(examId);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ExamSubjectsTest()
        {
            const string examId = "81ec269cd201481b98c749504bc7fe8b";
            var result = _examinationContract.ExamSubjects(examId);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void ClassAnalysisKeyTest()
        {
            const string examId = "81ec269cd201481b98c749504bc7fe8b";
            var result = _examinationContract.ClassAnalysisKey(new AnalysisInputDto
            {
                ExamId = examId,
                KeyType = 0,
                KeyScore = 500,
                ScoreA = 60,
                UnScoreA = 40
            });
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void ClassAnalysisLayerTest()
        {
            const string examId = "81ec269cd201481b98c749504bc7fe8b";
            var result = _examinationContract.ClassAnalysisLayer(new ClassAnalysisLayerInputDto
            {
                ExamId = examId,
                LayerA = 10,
                LayerB = 30,
                LayerC = 30,
                LayerD = 20,
                LayerE = 10
            });
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void SubjectAnalysisTest()
        {
            const string examId = "81ec269cd201481b98c749504bc7fe8b";
            var result = _examinationContract.SubjectAnalysis(new SubjectAnalysisInputDto
            {
                ExamId = examId,
                ExamSubjectId = "b001ee09fd9b4191955f7f963ae1a157",
                KeyType = 0,
                KeyScore = 80,
                ScoreA = 60,
                UnScoreA = 40
            });
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void SummaryTest()
        {
            const string examId = "81ec269cd201481b98c749504bc7fe8b";
            var result = _examinationContract.Summary(examId, 322936262624);
            Console.Write(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void ScoreRateTest()
        {
            const string examSubjectId = "17fbcabbca5b460d9a0f84ce649d5992";
            var result = _examinationContract.SubjectScoreRates(examSubjectId);
            WriteJson(result);
        }

        [TestMethod]
        public void UnionReportTest()
        {
            const long userId = 900000000002L;
            var ids = new List<string>
            {
                "778b3f839e214fa28d736bf5a09d96c0",
                "1e3542bd1272492c94ca56995e7b5fa0",
                "d3ba0fc5190d42c1bb0a96bda3699d82"
            };
            var result = _examinationContract.UnionReport(ids, userId);
            WriteJson(result);
        }

        [TestMethod]
        public void CancelUnionTest()
        {
            const string batch = "063dfbbbf65e4bafbca63e93d344315b";
            var result = _examinationContract.CancelUnion(batch);
            WriteJson(result);
        }

        [TestMethod]
        public void UnionListTest()
        {
            var result = _examinationContract.UnionList();
            WriteJson(result);
        }

        [TestMethod]
        public void UnionSourceTest()
        {
            var result = _examinationContract.UnionSource("1e5c5fa43d594d37af1b4f2fcd4adf32");
            WriteJson(result);
        }
    }
}
