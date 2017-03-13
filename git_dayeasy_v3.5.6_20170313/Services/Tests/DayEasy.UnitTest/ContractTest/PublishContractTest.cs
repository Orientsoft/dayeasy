using System;
using System.Collections.Generic;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Core.Domain;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class PublishContractTest : TestBase
    {

        private readonly IPublishContract _publishContract;

        public PublishContractTest()
        {
            _publishContract = Container.Resolve<IPublishContract>();
        }


        [TestMethod]
        public void GetTeacherPubWorksTest()
        {
            //900000000001
            var item = _publishContract.GetTeacherPubWorks(900000000002, null, DPage.NewPage(1, 10));
            Console.WriteLine(item);
        }

        [TestMethod]
        public void TestLoadByBatch()
        {
            var json = _publishContract.LoadByBatch("b1b584c98b8f4ac9ab0c4465d28e643b");
            Console.Write(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }


        [TestMethod]
        public void TestPublish()
        {
            const string paperId = "9b7d44ebfa65402888052c0d80336de8";
            const string groupIds = "[\"939e569677624e85a9fdb56d0ceaa212\",\"2ee1e85aa30f461ab52308b30c0b0bcc\"]";
            const long userId = 322936262622;

            var result = _publishContract.PulishPaper(paperId, groupIds, "", userId, "");

            Console.Write(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void TestVariant()
        {
            //var json =
            //_paperContract.AddVariant(900000000002, "7fcba522f63a4644b2fd4620fd268b0c", "0b9adc9bea794bfd8372dbb0cb7647ea", new List<string> { "0112b8e2a8604649a2b0f44f72c5eb79" });

            _publishContract.Variant("90bb6a26ec9345b096808f140125ff69", 20);
            //_paperContract.VariantRelationQuestion("0b9adc9bea794bfd8372dbb0cb7647ea", 4);
            //_paperContract.VariantHistory("7fcba522f63a4644b2fd4620fd268b0c", "0112b8e2a8604649a2b0f44f72c5eb79", 900000000002);
            //Console.Write(JsonHelper.ToJson(json.Data.Select(u => u.Tags), indented: true));
        }

        [TestMethod]
        public void VariantTest()
        {
            string str = "06dcb7e9e1274c23ba8784ae4b713cfd";
            const string qid = "06dcb7e9e1274c23ba8784ae4b713cfd";
            var excepts = new List<string> { "90bb6a26ec9345b096808f140125ff69",
                "82732de193f54ff3b1cf73b2a6bcf5f7",
                "44b06edce42c436ca43a575ca3d39670",
                "bd8aa419ddf54eb7af8f5a1df84b3cc4",
                "eeeb3c2ffcb8435ea7832e3e4def4cbb",
                "77804a6057794620b1d8389ba76d8e5b",
                "0734205c3ee24b47bddebb7913a22639",
                "d836d76a119f4b77b35f9457c09898f1",
                "1427ecd858a24dfca840bf628dd42c6e",
                "5e9ef47a0d8347c9901e6f68b343955b",
                "d5ccaf77046f4759b4be6522d2218019",
                "00763a98d9d34a77acc7e09b6fce78b5" };
            excepts = str.Split(',').ToList();
            var variant = _publishContract.Variant(qid, 1, excepts);
            Console.WriteLine(variant);
        }

        [TestMethod]
        public void VariantListTest()
        {
            const string batch = "82860d16b4be477aa1b2dab7afb4beed";
            const string paperId = "1b191bebd03c4bd285df7df8f0cd5197";
            var list = _publishContract.VariantList(batch, paperId);
            Console.WriteLine(JsonHelper.ToJson(list));
        }

        [TestMethod]
        public void VariantListFromSystemTest()
        {
            const string batch = "1872f1986db849f285b94d43ff6ad7a8";
            const string paperId = "f2ced0edc7244a3e9e67b1946f2ca9de";
            const long studentId = 0;
            var list = _publishContract.VariantListFromSystem(batch, paperId, studentId);
            Console.WriteLine(JsonHelper.ToJson(list));
        }

        [TestMethod]
        public void PaperWeakTest()
        {
            const string batch = "9de0ce7c1bfc4201b822f4060d104e96";
            const string paperId = "05e352cd41ce4ca097518151a6dda539";
            var list = _publishContract.PaperWeak(batch, paperId);
            Console.WriteLine(JsonHelper.ToJson(list));
        }
        [TestMethod]
        public void GetRecommendTutors()
        {
            const string batch = "31bda90c62aa4732bd5b329f18a68129";
            long userId = 1077834883015;
            var result = _publishContract.GetRecommendTutors(batch, userId);
            Console.WriteLine(result);
        }
    }
}
