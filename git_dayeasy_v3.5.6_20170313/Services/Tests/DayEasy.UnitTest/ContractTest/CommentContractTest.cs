using System;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.UnitTest.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class CommentContractTest : TestBase
    {
        private readonly ICommentContract _commentContract;
        private const string SourceId = "shoy123456";

        public CommentContractTest()
        {
            _commentContract = Container.Resolve<ICommentContract>();
        }

        [TestMethod]
        public void CommentTest()
        {
            var result = _commentContract.Comment(SourceId, 304533611023, "Hi shoy 你好！", "939c2e6183924e8b9ac4344abf21f313");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void CommentListTest()
        {
            var result = _commentContract.CommentList(new CommentSearchDto
            {
                SourceId = "80e1c57f8dc5470497cacda2a0749059",
                Page = 0,
                Size = 10
            });
            Console.WriteLine(result);
        }

        [TestMethod]
        public void DeleteTest()
        {
            var result = _commentContract.Delete("fdbe01cdc446431391f9ebb220ec7603", 304533611023);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void DialogTest()
        {
            var result = _commentContract.CommentDialog("939c2e6183924e8b9ac4344abf21f313");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void LikeTest()
        {
            var result = _commentContract.Like("939c2e6183924e8b9ac4344abf21f313", 329883067082);
            Console.WriteLine(result);
        }
    }
}
