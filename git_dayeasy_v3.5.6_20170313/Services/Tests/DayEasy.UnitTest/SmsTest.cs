using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using DayEasy.Contracts;
using DayEasy.Core.Dependency;
using DayEasy.UnitTest.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DayEasy.UnitTest
{
    [TestClass]
    public class SmsTest : TestBase
    {
        private ISmsRecordContract _contract;

        public SmsTest()
        {
            _contract = Container.Resolve<ISmsRecordContract>();
        }

        [TestMethod]
        public void SendSmsTest()
        {
            var mobiles = new List<string> {"15828221226", "18782246531"};
            var messages = new List<string>
            {
                "【得一教育】罗Shoy，语文《第一周周练习》得分为38.5，班内平均分为69.8，详情请登录www.dayeasy.net",
                "【得一教育】您在语文《第一周周练习》中得分为38.5，班内平均分为69.8，详情请登录www.dayeasy.net"
            };

            var result = _contract.SendLotSize(mobiles, messages);
            Console.Write(result);

        }
    }
}
