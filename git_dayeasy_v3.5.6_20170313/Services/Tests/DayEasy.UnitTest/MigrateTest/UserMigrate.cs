using System;
using Autofac;
using DayEasy.Contracts;
using DayEasy.UnitTest.TestUtility;
using DayEasy.User.Services.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.MigrateTest
{
    [TestClass]
    public class UserMigrate : TestBase
    {
        private readonly ITempOldContract _tempOldContract;

        public UserMigrate()
        {
            _tempOldContract = Container.Resolve<ITempOldContract>();
        }

        [TestMethod]
        public void GenerateDCode()
        {
            var result = _tempOldContract.UpdateDCode();
            Console.WriteLine(result);
        }

        [TestMethod]
        public void AddApplicationTest()
        {
            UserCache.Instance.RemoveApps();
            var result = _tempOldContract.AddApplication(304533611023, 1001);
            Console.WriteLine(result);
        }
    }
}
