using System;
using System.Diagnostics;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Core.Domain;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class ErrorQuestionContractTest : TestBase
    {
        private readonly IErrorBookContract _errorBookContract;

        public ErrorQuestionContractTest()
        {
            _errorBookContract = Container.Resolve<IErrorBookContract>();
        }

        [TestMethod]
        public void TestErrorQuestions()
        {
            var search = new ErrorQuestionSearchDto
            {
                Page = DPage.NewPage(0,10),
                StudentId = 322936262625,
                SubjectId = 1,
                QType = 1,
                StartTime = new DateTime(2015, 9, 1),
                EndTime = new DateTime(2015, 9, 30),
            };
            var json = _errorBookContract.ErrorQuestions(search);
            Console.Write(JsonHelper.ToJson(json, NamingType.UrlCase, true));
        }

        [TestMethod]
        public void TestErrorQuestion()
        {
            var json = _errorBookContract.ErrorQuestion("23b7f2b2f9154561a98f0ca11cd18084", 322936262625);
            Console.Write(JsonHelper.ToJson(json, NamingType.UrlCase, true));
        }

        [TestMethod]
        public void TestErrorQuestionRate()
        {
            var json = _errorBookContract.ErrorQuestionRate(
                "1e2d4db019bc4e6d9bda910f76222931",
                "135d3f4cf1464243b887779b6e921613",
                "afbf2793fd9c444fb6b0362a21114616");
            Console.Write(JsonHelper.ToJson(json, NamingType.UrlCase, true));

        }

        [TestMethod]
        public void TestComments()
        {
            var json = _errorBookContract.Comments(DPage.NewPage(0, 10), "df302577b2a54229b80fb88d8b7e5e89", "");
            Console.Write(JsonHelper.ToJson(json, NamingType.UrlCase, true));
        }

        [TestMethod]
        public void TestErrorAnswer()
        {
            var json = _errorBookContract.ErrorAnswer(
                "aba0f62afe8c46acac8a1ef381f8a582",
                "24bacffb5a564b71a2a6df41b2207754",
                "2f8d1b37d8cd4a68bd50ccfe1fd6e004", 
                322936262625);
            Console.Write(JsonHelper.ToJson(json, NamingType.UrlCase, true));
        }

        [TestMethod]
        public void TestReasonCountDict()
        {
            var watch = new Stopwatch();
            watch.Start();
            var json = _errorBookContract.ReasonCountDict("597fd169ba9b42daa4be13c955d446e4");
            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalMilliseconds);
            Console.WriteLine(json.ToJson());
        }
        [TestMethod]
        public void ErrorQuestions()
        {
            SearchErrorQuestionDto dto = new SearchErrorQuestionDto();
           // dto.KnowledgeCode = "0";
            dto.QuestionType = -1;
            dto.DateRange = -1;
            dto.pageIndex = 0;
            dto.pageSize = 8;
            dto.SubjectId =2;
            dto.UserId = 0;
            dto.GroupId = "f441f5c4f7e546509d2041d3c2afe144";
             dto.KnowledgeCode = "0";
            dto.OrderOfArr = 0;
              var result = _errorBookContract.ErrorQuestions(dto);
            WriteJson(result);
        }

    }
}
