using System;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Models;
using DayEasy.Core.Events;
using DayEasy.Core.Events.EventData;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class LogContractTest : TestBase
    {
        private readonly ILogger _logger = LogManager.Logger<UtilityTest>();
        private readonly ILogContract _logContract;
        private readonly IEventsManager _eventsManager;

        public LogContractTest()
        {
            _logContract = Container.Resolve<ILogContract>();
            _eventsManager = Container.Resolve<IEventsManager>();
        }
        [TestMethod]
        public void EventsTest()
        {
            var unregister = _eventsManager.Register<UpdatedEventData<TL_UserLog>>(t =>
            {
                Task.Factory.StartNew(() =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine("update event:");
                    sb.AppendLine("eventTime:" + t.EventTime);
                    sb.AppendLine("detail:" + t.Entity.LogDetail);
                    _logger.Info(sb);
                });
            });

            const string detail = "用户[1162131470@qq.com]，于2015-09-16 09:18:01登录，状态为登录成功";
            using (unregister)
            {
                var repository = _logContract.UserLogRepository;
                //                var result = repository.Update(t => t.Id == 618094854612, t => new TL_UserLog
                //                {
                //                    LogDetail = "用户[1162131470@qq.com]，于2015-09-16 09:18:01登录，状态为登录成功"
                //                });
                var item = repository.Load(618094854612);
                item.LogDetail = detail;
                var result = repository.Update(t => new
                {
                    t.LogDetail
                }, item);
                Console.WriteLine(result);
            }
        }
    }
}
