using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Core.Cache;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FrameworkTest
{
    [TestClass]
    public class CacheTest : TestBase
    {
        [TestMethod]
        public void TestMethod1()
        {
            const string region = "shay";
            const string key = "key";
            var cache = CacheManager.GetCacher(region);
            cache.Set(key, "shay 123");
            var word = cache.Get<string>(key);
            Console.WriteLine(word);
        }

        [TestMethod]
        public void ClearCache()
        {
            //var manager = RedisManager.Instance;
            //using (var cache = manager.GetClient())
            //{
            //    var keys = cache.SearchKeys("markingPic:*");
            //    Console.WriteLine(keys.Count);
            //    cache.RemoveAll(keys);
            //}
        }

        [TestMethod]
        public void GetCacheTest()
        {
            var manager = CacheManager.GetCacher("paper");
            const string paperId = "040cd139b31144fa9d28e85cf5538013";
            var sections = manager.Get<List<PaperSectionDto>>(paperId);
            Console.WriteLine(JsonHelper.ToJson(sections));
            sections = manager.Get<List<PaperSectionDto>>(paperId);
            Console.WriteLine(JsonHelper.ToJson(sections));
        }
    }
}
