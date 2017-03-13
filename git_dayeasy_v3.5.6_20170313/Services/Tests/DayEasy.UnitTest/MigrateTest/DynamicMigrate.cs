
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.UnitTest.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.MigrateTest
{
    /// <summary> 动态转移 </summary>
    [TestClass]
    public class DynamicMigrate : TestBase
    {
        private readonly ITempContract _tempContract;
        private readonly ITempOldContract _tempOldContract;

        public DynamicMigrate()
        {
            _tempContract = Container.Resolve<ITempContract>();
            _tempOldContract = Container.Resolve<ITempOldContract>();
        }

        [TestMethod]
        public void Main()
        {
            var size = 100;
            int page = 0;
            int count = 0;
            while (true)
            {
                var list = _tempOldContract.LoadDynamics(page, size);
                if (list == null || !list.Any())
                    break;
                var dynamics = new List<TM_GroupDynamic>();
                foreach (var item in list)
                {
                    var dynamic = new TM_GroupDynamic
                    {
                        Id = item.Id,
                        AddedAt = item.SendTime,
                        AddedBy = item.SendUserID,
                        ContentType = (byte) ContentType.Publish,
                        ContentId = item.Batch,
                        DynamicType = (byte) (item.NewsType == 2 ? 1 : 0),
                        CommentCount = 0,
                        GoodCount = item.GoodCount,
                        GroupId = item.RecieveID,
                        Message = item.Remarks,
                        ReceiveRole = (byte) (UserRole.Student | UserRole.Teacher),
                        Status = (byte) NormalStatus.Normal
                    };
                    dynamics.Add(dynamic);
                }
                var result = _tempContract.BatchInsertDynamics(dynamics);
                if (result.Status)
                    count += result.Data;
                page++;
            }
            Console.WriteLine(count);
        }
    }
}
