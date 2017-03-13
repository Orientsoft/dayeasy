using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.MigrateTest
{
    /// <summary> 转移机构 </summary>
    [TestClass]
    public class AgencyMigrate : TestBase
    {
        private readonly ITempContract _tempContract;
        private readonly ITempOldContract _tempOldContract;

        public AgencyMigrate()
        {
            _tempContract = Container.Resolve<ITempContract>();
            _tempOldContract = Container.Resolve<ITempOldContract>();
        }

        [TestMethod]
        public void Main()
        {
            var list = _tempOldContract.AgencyList(DPage.NewPage(0, 2000));
            if (!list.Status)
                Console.WriteLine(list.Message);
            var agencies = new List<TS_Agency>();
            foreach (var agency in list.Data)
            {
                agencies.Add(new TS_Agency
                {
                    Id = agency.Id,
                    AgencyName = agency.AgencyName,
                    AgencyLogo = agency.AgencyLogo,
                    AgencyType = agency.AgencyType,
                    AreaCode = agency.Area ?? 0,
                    Sort = 0,
                    Status = 0
                });
            }
            var result = _tempContract.CreateAgencies(agencies);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void Import()
        {
            var list = File.ReadAllLines("agency.txt");
            var agencies = new List<TS_Agency>();
            foreach (var item in list)
            {
                string[] array;
                if (string.IsNullOrWhiteSpace(item) || (array = item.Split(',')).Length != 3)
                    continue;
                agencies.Add(new TS_Agency
                {
                    Id = IdHelper.Instance.Guid32,
                    Stage = array[0].To((byte)0),
                    AgencyName = array[1],
                    AgencyType = (byte)AgencyType.K12,
                    Status = (byte)NormalStatus.Normal,
                    AreaCode = array[2].To(0)
                });
            }
            //            Console.WriteLine(JsonHelper.ToJson(agencies, NamingType.CamelCase, true));
            var result = _tempContract.CreateAgencies(agencies);
            Console.WriteLine(result);
        }
    }
}
