using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.ThirdPlatform.Entity.Config;
using DayEasy.UnitTest.TestUtility;
using DayEasy.User.Services.Helper;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class UserContractTest : TestBase
    {
        private readonly IUserContract _userContract;

        public UserContractTest()
        {
            _userContract = Container.Resolve<IUserContract>();
        }

        #region user
        [TestMethod]
        public void ServiceTest()
        {
            var user = _userContract.UserNames(new[] { 304533611023, 922936263642 });
            var task1 = Task.Factory.StartNew(() =>
            {
                var item = CurrentIocManager.Resolve<IUserContract>().Load(304533611023);
                Console.WriteLine(item.Name);
                var q = CurrentIocManager.Resolve<IPaperContract>().LoadQuestion("1");
                Console.WriteLine(q.Body);
            });
            var task2 = Task.Factory.StartNew(() =>
            {
                var item = CurrentIocManager.Resolve<IUserContract>().Load(304533611023);
                Console.WriteLine(item.Name);
                var q = CurrentIocManager.Resolve<IPaperContract>().LoadQuestion("2");
                Console.WriteLine(q.Body);
            });
            Task.WaitAll(task1, task2);
            Console.WriteLine(JsonHelper.ToJson(user, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void UserCacheTest()
        {
            //            var user = _userContract.Load("c1ac1108402f402fadc83dfb11c013fb");
            var user = UserCache.Instance.Get("de7b923c89904e16ac074325a344464d");
            Console.WriteLine(user);
        }

        [TestMethod]
        public void LoginTest()
        {
            var result = _userContract.Login(new LoginDto
            {
                Account = "634330628@qq.com",
                Password = "1234567",
                Comefrom = Comefrom.Android
            });
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ChangePwdTest()
        {
            var result = _userContract.ChangPwd("634330628@qq.com", "a123456", "a123456");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void PlatformsTest()
        {
            var type = _userContract.Platforms(329883067082, (int)PlatformType.Weibo);
            Console.WriteLine(JsonHelper.ToJson(type, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void ThirdLoginTest()
        {
            var result = _userContract.Login(new PlatformDto
            {
                PlatformId = "966891DDBB706579935995E591CDD61F",
                AccessToken = "10D29E2C752B3E85DC1992ECEA0EE044",
                PlatformType = 0
            }, Comefrom.Web);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void UserAppsTest()
        {
            UserCache.Instance.RemoveApps();
            var apps = _userContract.UserApplications(304533611023);
            Console.WriteLine(JsonHelper.ToJson(apps, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void LoadChildTest()
        {
            var result = _userContract.LoadChild("1371683491@qq.com", "a123456");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ParentBindTest()
        {
            var result = _userContract.BindChild(668220889185, 329883067082, FamilyRelationType.Father);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ParentUnBindTest()
        {
            var result = _userContract.CancelBindRelation(668220889185, 329883067082);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ParentsTest()
        {
            var result = _userContract.Children(668220889185);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));

            result = _userContract.Parents(329883067082);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void UserSearchTest()
        {
            var result = _userContract.Search("勇", DPage.NewPage(0, 10), (int)UserRole.Student);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void UpdateTest()
        {
            var user = new UserDto
            {
                Id = 411013378387,
                Role = 4,
                SubjectId = 9
            };
            var result = _userContract.Update(user);
            Console.WriteLine(result);
        }
        #endregion

        #region UsrAgency
        [TestMethod]
        public void AddAgencyTest()
        {
            var result = _userContract.AddAgency(new UserAgencyInputDto
            {
                UserId = 304533611023L,
                AgencyId = "aed02ded6fc2495ba4d6c7e2f372b225",
                Status = (byte)UserAgencyStatus.Current,
                Start = new DateTime(2016, 9, 1),
                End = null
            });
            WriteJson(result);
        }

        [TestMethod]
        public void DeleteAgencyTest()
        {
            var result = _userContract.RemoveAgency("61aa854119554638b276353bd426f07a", 304533611023L);
            WriteJson(result);
        }

        [TestMethod]
        public void AgencyListTest()
        {
            const long userId = 304533611023L;
            var list = _userContract.AgencyList(userId, DPage.NewPage(0, 3));
            WriteJson(list);
        }
        #endregion

        [TestMethod]
        public void AddImpressionTest()
        {
            var dto = new ImpressionInputDto
            {
                CreatorId = 304533611023L,
                UserId = 220000000001L,
                Content = new List<string>
                {
                    "黄金左手",
                    "漂亮暖男"
                }
            };
            var result = _userContract.AddImpression(dto);
            WriteJson(result);
        }

        [TestMethod]
        public void SupportImpressionTest()
        {
            const string id = "6d064b48011a4f5fa91cb1210b98028b";
            var result = _userContract.SupportImpression(id, 220000000001L);
            WriteJson(result);
        }

        [TestMethod]
        public void AddQuotationsTest()
        {
            var dto = new QuotationsInputDto
            {
                CreatorId = 220000000001L,
                UserId = 322936262622L,
                Content = "年少的时候你做了一个决定要把生命献给爱情，后来你没死，年轻替你抵了命。"
            };
            var result = _userContract.AddQuotations(dto);
            WriteJson(result);
        }

        [TestMethod]
        public void SupportQuotationsTest()
        {
            const string id = "56cbf8b730c943748253451180055101";
            var result = _userContract.SupportQuotations(id, 304533611023L);
            WriteJson(result);
        }
        [TestMethod]
        public void LoadUserByAgency()
        {
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            const int role = (int)UserRole.Teacher;
            var result=    _userContract.LoadUsersByAgencyId(agencyId, role);
            WriteJson(result);
        }
        [TestMethod]
        public void LoadTeacherTest() {
            const int subjectId = 0;
            string t = 0.ToString();
            const string agencyId = "aed02ded6fc2495ba4d6c7e2f372b225";
            var result = _userContract.LoadTeacher(subjectId, agencyId,DPage.NewPage(0,10));
            WriteJson(result);
        }
    }
}
