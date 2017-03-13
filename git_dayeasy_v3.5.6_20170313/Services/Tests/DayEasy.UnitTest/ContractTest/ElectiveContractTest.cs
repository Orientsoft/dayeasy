using System;
using Autofac;
using DayEasy.Contracts;
using DayEasy.UnitTest.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class ElectiveContractTest : TestBase
    {
        private readonly IElectiveContract _electiveContract;

        public ElectiveContractTest()
        {
            _electiveContract = Container.Resolve<IElectiveContract>();
        }

        [TestMethod]
        public void AgencyCourseTest()
        {
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            const long userId = 322936262625L;
            var batch = _electiveContract.AgencyCourse(agencyId, userId);
            Console.WriteLine(batch);
        }
    }
}
