using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Download;
using DayEasy.Contracts.Dtos.ErrorQuestion;
using DayEasy.Contracts.Dtos.SchoolBook;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core.Dependency;
using DayEasy.MongoDb;
using DayEasy.Services;
using DayEasy.Services.Helper;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class SystemContractTest : TestBase
    {
        private readonly ISystemContract _systemContract;

        public SystemContractTest()
        {
            _systemContract = Container.Resolve<ISystemContract>();
        }

        [TestMethod]
        public void SubjectTest()
        {
            var subs = SystemCache.Instance.Subjects();
            Console.WriteLine(JsonHelper.ToJson(subs, indented: true));
        }

        [TestMethod]
        public void QuestiontypesTest()
        {
            var types = SystemCache.Instance.SubjectQuestionTypes(5); //_systemContract.GetQuTypeBySubjectId(5);
            Console.WriteLine(JsonHelper.ToJson(types, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void CacheTest()
        {
            var cache = SystemCache.Instance;
            var subjects = cache.Subjects();
            var types = cache.SubjectQuestionTypes(5);
            Console.WriteLine(JsonHelper.ToJson(subjects, NamingType.CamelCase, true));
            Console.WriteLine(JsonHelper.ToJson(types, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void KnowlwdgesTest()
        {
            var list = _systemContract.Knowledges(new SearchKnowledgeDto
            {
                Stage = 2,
                SubjectId = 5,
                ParentId = -1,
                ParentCode = "20502",
                Keyword = "地球",
                Page = 0,
                Size = 15
            });
            Console.WriteLine(JsonHelper.ToJson(list, NamingType.CamelCase, true, props: new[] { "code", "name" }));
        }

        [TestMethod]
        public void AgencyListTest()
        {
            var list = _systemContract.AgencyList(StageEnum.JuniorMiddleSchool, 5101, AgencyType.K12).ToList();
            Console.WriteLine(list.Count());
            Console.WriteLine(JsonHelper.ToJson(list, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void AddApplicationTest()
        {
            var repository = CurrentIocManager.Resolve<IDayEasyRepository<TS_Application, int>>();
            var result = repository.Insert(new TS_Application
            {
                AppName = "测试应用",
                AppURL = "question/add",
                IsSLD = false,
                AppIcon = "dy-icon-chujuan",
                Sort = 10,
                AppType = 4,
                AppRoles = 6,
                Status = 0,
                AppRemark = "测试"
            });
            Console.WriteLine(result);
        }

        #region 教材章节

        [TestMethod]
        public void AddSchoolBookTest()
        {
            var json = _systemContract.AddSchoolBook(new SchoolBookDto
            {
                Stage = 2,
                SubjectId = 2,
                Title = "测试版3"
            });
            Console.Write(json.ToJson());
        }

        [TestMethod]
        public void DelSchoolBookTest()
        {
            //Console.Write(_systemContract.EditSchoolBook(new SchoolBookDto
            //{
            //    Id = "21a52b127bdb4f488899bf2b1e521826",
            //    Status = (byte) NormalStatus.Delete
            //}));
            var code = "C0011223344";
            var codes = new List<string> { code };
            while (code.Length > 4)
            {
                code = code.Substring(0, code.Length - 2);
                if (code.Length > 4) codes.Add(code);
            }
            Console.Write(codes.ToJson());
        }

        [TestMethod]
        public void SbChapterKnowledgesTest()
        {
            var json = _systemContract.SbChapterKnowledges("203010103");
            Console.Write(json);
        }

        #endregion

        [TestMethod]
        public void DownloadLogTest()
        {
            var collection = new MongoManager().Collection<MongoDownloadLog>();
            var count = collection.Count();
            Console.WriteLine(count);
            var list = collection.Find(Query.Empty)
                .SetSortOrder(SortBy.Ascending(nameof(MongoDownloadLog.AddedAt)))
                .SetSkip(0)
                .SetLimit(2);
            Func<string, DownloadType> convertType = type =>
            {
                switch (type)
                {
                    case "试卷":
                        return DownloadType.Paper;
                    case "错题":
                        return DownloadType.ErrorQuestion;
                    case "变式题":
                        return DownloadType.Variant;
                    case "考试统计":
                        return DownloadType.ClassStatistic;
                    case "分数段排名":
                        return DownloadType.ClassSegment;
                    case "教务管理-班级分析":
                        return DownloadType.ClassAnalysis;
                    case "教务管理-学科分析":
                        return DownloadType.SubjectAnalysis;
                    case "教务管理-年级排名":
                        return DownloadType.GradeRank;
                }
                return DownloadType.ErrorQuestion;
            };
            foreach (var item in list)
            {
                var log = _systemContract.CreateDownload(new DownloadLogInputDto
                {
                    UserId = item.UserId,
                    Agent = item.UserAgent,
                    Referer = item.Referer,
                    Count = 1,
                    Type = convertType(item.Type)
                });
                var result = _systemContract.CompleteDownload(log);
                WriteJson(result);
            }
        }
        [TestMethod]
        public void Knowledges()
        {
            SearchErrorQuestionDto dto = new SearchErrorQuestionDto();
            dto.SubjectId =5;
            dto.GroupId = "4e8eb98d82bf4ceab3dcb08f6b221675";
            //dto.SubjectId = 3;
            var result = _systemContract.Knowledges(dto);
            WriteJson(result);
        }
        [TestMethod]
        public void ErrorUsers()
        {
            const string groupid = "b92fb10e96bc4624beea3a12492668c5";
            var result = _systemContract.ErrorUsers(groupid,2);
            WriteJson(result);
        }
    }
}
