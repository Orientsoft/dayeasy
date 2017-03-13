using System;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Cache;
using DayEasy.MemoryDb.Configs;
using DayEasy.MemoryDb.Redis;
using DayEasy.Paper.Services.Helper.Question;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class QuestionContractTest : TestBase
    {
        private readonly IPaperContract _paperContract;

        public QuestionContractTest()
        {
            _paperContract = Container.Resolve<IPaperContract>();
        }

        [TestMethod]
        public void ConvertTest()
        {
            var item = _paperContract.LoadQuestion("2d249d5d76944f3aaf298cbdc0c90d2b", false);
            Console.WriteLine(item);
            //            _paperContract.SaveQuestion(item);
        }

        [TestMethod]
        public void SearchTest()
        {
            var list = _paperContract.SearchQuestion(new SearchQuestionDto
            {
                UserId = 304533611023,
                Page = 1,
                Size = 10,
                Order = QuestionOrder.AddedAtDesc
            });
            Console.WriteLine(list);
        }

        [TestMethod]
        public void SaveTest()
        {
            var body =
                "{\"id\":\"ee54eef246b94107b6a0484e4ebe02de\",\"body\":\"等腰三角形的两边长分别为4和9，则这个三角形的周长为（&#160;&#160;&#160;&#160;）&#160;&#160;&#160;&#160;&#160; \",\"type\":1,\"subjectId\":5,\"stage\":2,\"optionStyle\":2,\"hasSmall\":false,\"isUsed\":true,\"knowledges\":{\"205021018\":\"酸碱反应\"},\"tags\":[\"等腰三角形\",\"周长\",\"三角形\"],\"images\":[],\"difficulty\":3,\"time\":\"2015-06-30\",\"showOption\":true,\"isObjective\":true,\"userId\":304533611023,\"userName\":null,\"range\":0,\"useCount\":0,\"answerCount\":7,\"errorCount\":4,\"details\":[],\"answers\":[{\"id\":\"77d369defee24fb487b67c6197fac9ac\",\"body\":\"13\",\"images\":[],\"sort\":0,\"tag\":\"A\",\"isCorrect\":false},{\"id\":\"3ac1a2e9abab4c6db074a9a6ad57c73a\",\"body\":\"17\",\"images\":[],\"sort\":1,\"tag\":\"B\",\"isCorrect\":false},{\"id\":\"d9e25c6e45534bf9ba79ffdb6442a5cc\",\"body\":\"22\",\"images\":[],\"sort\":2,\"tag\":\"C\",\"isCorrect\":true},{\"id\":\"a535d8fdeb2e4ed8b6ff31eb4444479f\",\"body\":\"17或22a\",\"images\":[],\"sort\":3,\"tag\":\"D\",\"isCorrect\":false}],\"analysis\":null}";
            var dto = JsonHelper.Json<QuestionDto>(body, NamingType.CamelCase);
            var result = _paperContract.SaveQuestion(dto);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void QuestionCacheTest()
        {
            QuestionCache.Instance.Remove("a33795b3c2294dd6a232d4b683711154");
        }

        [TestMethod]
        public void QuestionAnswerTest()
        {
            var answers = _paperContract.QuestionAnswer("5c3b99cd6bba4f9d90e444b672d46072", "dd97a4ee44e44257a976bf91697879df");
            Console.WriteLine(answers.ToJson());
        }

        [TestMethod]
        public void QuestionSortsTest()
        {
            var sorts = _paperContract.QuestionSorts("a2a57dc7c8e24b8c860dc744f517a71b");
            Console.WriteLine(sorts.ToJson());
        }

        [TestMethod]
        public void LoadQuestionTest()
        {
            var dto = _paperContract.LoadQuestion("b1f175511fd24af3ab523a086757ff1f", false);
            Console.WriteLine(dto);
        }
    }
}