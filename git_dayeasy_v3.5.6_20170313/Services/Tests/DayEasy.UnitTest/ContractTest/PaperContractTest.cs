using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Models;
using DayEasy.Core.Cache;
using DayEasy.Core.Dependency;
using DayEasy.Paper.Services.Helper;
using DayEasy.Services;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class PaperContractTest : TestBase
    {
        private readonly IPaperContract _paperContract;
        private readonly IMarkingContract _markingContract;

        public PaperContractTest()
        {
            _paperContract = Container.Resolve<IPaperContract>();
            _markingContract = Container.Resolve<IMarkingContract>();
        }

        [TestMethod]
        public void DtoTest()
        {
            var item = _paperContract.LoadQuestion("ee3490ab49f6459499f3a00bbc62e236");
            Console.WriteLine(item);
            //                var qItem = item.MapTo<TQ_Question>();
            //                Console.WriteLine(JsonHelper.ToJson(qItem, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void PaperListTest()
        {
            //335603683145
            var result = _paperContract.PaperList(new SearchPaperDto()
            {
                UserId = 335603683145,
                Key = "测量的"
            });

            Console.Write(result);
        }

        [TestMethod]
        public void PaperDetailByIdTest()
        {
            //0064c3f755024f2da7a5bfb5a441c06e
            var result = _paperContract.PaperDetailById("4462eb94f18f42b0bf23e3c63d5196b1");
            Console.Write(JsonHelper.ToJson(result, indented: true));
        }

        [TestMethod]
        public void PaperDetailByPaperNoTest()
        {
            //15030392906
            var result = _paperContract.PaperDetailByPaperNo("15030392906");
            Console.Write(result);
        }


        [TestMethod]
        public void GetPaperAnswersTest()
        {
            //0064c3f755024f2da7a5bfb5a441c06e
            var result = _paperContract.GetPaperAnswers("0064c3f755024f2da7a5bfb5a441c06e");
            Console.Write(result);
        }

        [TestMethod]
        public void GetDraftCountTest()
        {
            //335603683145
            //var result = _paperContract.GetDraftCount(335603683145, 5);
            //Console.Write(result);
        }

        [TestMethod]
        public void LoadQuestionTest()
        {
            //var result = _paperContract.LoadQuestion("913c2f8004d649ebbe7b54a747652f59");
            //Console.Write(result);
            //更新知识点为字典
            var repository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();
            var questions = repository.Where(u => u.KnowledgeIDs.Contains("name")).ToList();

            questions.ForEach(q =>
            {
                var kps = JsonHelper.JsonList<NameDto>(q.KnowledgeIDs).DistinctBy(u => u.Id).ToList();
                q.KnowledgeIDs = kps.ToDictionary(u => u.Id, u => u.Name).ToJson();

                repository.Update(t => new { t.KnowledgeIDs }, q);
            });

            Console.WriteLine("ok");
        }


        [TestMethod]
        public void EditPaperAnswerTest()
        {
            var answers = "[{\"QId\":\"4dbf418a35a649788d401458e75abd61\",\"Answer\":\"B\"},{\"QId\":\"4cd3d97764a1450fab250ee9d806cd5c\",\"Answer\":\"B\"},{\"QId\":\"5a7751a8625842cd925a248e5ba0dc21\",\"Answer\":\"B\"},{\"QId\":\"a272396d69944e7787cca266f3fc521f\",\"Answer\":\"D\"},{\"QId\":\"7759760927884a69b903d791cb0fb49f\",\"Answer\":\"C\"},{\"QId\":\"a48559ea9ad2412d835aae7a914037ea\",\"Answer\":\"A\"},{\"QId\":\"a895abfd0aa7483a84cef1b9bc37bdd9\",\"Answer\":\"C\"},{\"QId\":\"92b0a4344c854a17a75763ebc4bf3f4b\",\"Answer\":\"D\"},{\"QId\":\"e6f70ec1ba5e4771be2965e49471537d\",\"Answer\":\"B\"},{\"QId\":\"a7085d24b32d4d53878436883551e3ea\",\"Answer\":\"B\"},{\"QId\":\"50eb522749df41329fdda07f05593bba\",\"Answer\":\"A \"},{\"QId\":\"3adce99af76c40e3b411fc4453ae48c7\",\"Answer\":\"A  \"},{\"QId\":\"66552ae190b94cae9190cf72c2314ab2\",\"Answer\":\"A  \"},{\"DetailId\":\"1325395e208f465a8e0f43141b05a397\",\"QId\":\"e6f360c48b904e9e9d8596950a08f265\",\"Answer\":\"A\"},{\"DetailId\":\"f7ef0eb438e74445b37e0e017815cab6\",\"QId\":\"e6f360c48b904e9e9d8596950a08f265\",\"Answer\":\"A\"},{\"QId\":\"259d87cabcb948c3893232d86a5633a5\",\"Answer\":\"&lt;p&gt;略&lt;/p&gt;\"},{\"QId\":\"d8d467355f344240bf9dd417143e911b\",\"Answer\":\"\\n                                                                \\n20                    \\n\\n                                                    \"},{\"QId\":\"3d158fcf39904546b9a420ae2a6efb21\",\"Answer\":\"\\n                                                                \\n0.2                    \\n\\n                                                    \"},{\"QId\":\"669c7d77bdba42c7832eec3f90678195\",\"Answer\":\"\\n                                                                \\nffre                    \\n\\n                                                    \"},{\"QId\":\"1094517d2f0149ffb3d522cb06952bdb\",\"Answer\":\"\\n                                                                \\ng                    \\n\\n                                                    \"},{\"QId\":\"2c7386c16fc842f5b5e07f018bbf04d7\",\"Answer\":\"\\n                                                                \\n&lt;span&gt;略&lt;/span&gt;                    \\n\\n                                                    \"},{\"QId\":\"a022f96133004428b668721e285ddb5a\",\"Answer\":\"\\n                                                                \\n解：（1）27×33，&lt;br/&gt;=（30-3）（30+3），&lt;br/&gt;=30&lt;sup&gt;2&lt;/sup&gt;-3&lt;sup&gt;2&lt;/sup&gt;&lt;br/&gt;=891；&lt;br/&gt;（2）5.9×6.1，&lt;br/&gt;=（6-0.1）（6+0.1），&lt;br/&gt;=6&lt;sup&gt;2&lt;/sup&gt;-（0.1）&lt;sup&gt;2&lt;/sup&gt;，&lt;br/&gt;=35.99；&lt;br/&gt;（3）99×101，&lt;br/&gt;=（100-1）（100+1），&lt;br/&gt;=100&lt;sup&gt;2&lt;/sup&gt;-1&lt;sup&gt;2&lt;/sup&gt;，&lt;br/&gt;=9999；&lt;br/&gt;（4）1005×995，&lt;br/&gt;=（1000+5），&lt;br/&gt;=1000&lt;sup&gt;2&lt;/sup&gt;-5&lt;sup&gt;2&lt;/sup&gt;，&lt;br/&gt;=999975．                    \\n\\n                                                    \"},{\"QId\":\"fdd1729c9eee47bfa67aef918c9e5f85\",\"Answer\":\"\\n                                                                \\n&lt;span&gt;略&lt;/span&gt;                    \\n\\n                                                    \"},{\"QId\":\"cc11986bf6e5417cb6855ee6d01c0b02\",\"Answer\":\"\\n                                                                    \\n&lt;span&gt;略&lt;/span&gt;                    \\n\\n                                                        \"},{\"QId\":\"7f31aded11ab4e4c8fc3552b7770e3a8\",\"Answer\":\"\\n                                                                    \\n&lt;span&gt;略&lt;/span&gt;                    \\n\\n                                                        \"}]";

            answers = answers.HtmlDecode();
            var answerList = JsonHelper.Json<List<MakePaperAnswerDto>>(answers).ToList();

            var result = _paperContract.EditPaperAnswer("083894fdf10747bf9dd33f162598b0c9", answerList, 334095189371);

            //_markingContract.UpdateQuestionMarking("467ca00b9ec742f0bcb3deba9a99391e", "185935e5196e49759cad3bddb8b03331");

            Console.WriteLine("Ok");
        }

        [TestMethod]
        public void DeleteQuestionTest()
        {
            var json = _paperContract.DeleteQuestion("4011255cceec4ed893ff68b0834985a4", 900000000001);
            Console.WriteLine(json);
        }

        [TestMethod]
        public void PaperSortCacheTest()
        {
            const string paperId = "083894fdf10747bf9dd33f162598b0c9";
            var result = PaperHelper.PaperSorts(paperId);
            WriteJson(result);
        }

        [TestMethod]
        public void LoadQuestionsTest()
        {
            //CacheManager.GetCacher("user").Clear();
            //CacheManager.GetCacher("paper").Clear();
            //CacheManager.GetCacher("question").Clear();
            //CacheManager.GetCacher("token").Clear();
            //CacheManager.GetCacher("apps").Clear();
            //CacheManager.GetCacher("system").Clear();
            //return;
            const string paperId = "083894fdf10747bf9dd33f162598b0c9";
            var dto = _paperContract.PaperDetailById(paperId).Data;
            WriteJson(dto.PaperSections.Select(t => new
            {
                type = t.PaperSectionType,
                t.Sort,
                questions = t.Questions.Select(q => new
                {
                    q.Question.Id,
                    q.Sort,
                    details = q.Question.Details.IsNullOrEmpty() ? null : q.Question.Details.Select(d => d.Sort)
                })
            }));
        }

    }
}
