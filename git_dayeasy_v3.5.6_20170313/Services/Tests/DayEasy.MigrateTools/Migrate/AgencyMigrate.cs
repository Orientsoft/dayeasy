using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Download;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core.Domain;
using DayEasy.MongoDb;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;

namespace DayEasy.MigrateTools.Migrate
{
    /// <summary> 转移机构 </summary>
    public class AgencyMigrate : MigrateBase
    {
        private readonly ITempContract _tempContract;
        private readonly ITempOldContract _tempOldContract;
        private readonly ISystemContract _systemContract;

        public AgencyMigrate()
        {
            _tempContract = Container.Resolve<ITempContract>();
            _tempOldContract = Container.Resolve<ITempOldContract>();
            _systemContract = Container.Resolve<ISystemContract>();
        }

        /// <summary> 导入现有机构 </summary>
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

        /// <summary> 导入机构中同科目教师到一个同事圈 </summary>
        public void AgencyColleague()
        {
        }

        /// <summary> 导入成都市k12机构 </summary>
        public void Import()
        {
            var list = "agency.txt".ReadConfig();
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

        /// <summary> 转移下载日志 </summary>
        public void DownloadLog()
        {
            var collection = new MongoManager().Collection<MongoDownloadLog>();
            var count = collection.Count();
            Console.WriteLine(count);
            var list = collection.Find(Query.Empty)
                .SetSortOrder(SortBy.Ascending(nameof(MongoDownloadLog.AddedAt)));
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
                    case "协同统计":
                        return DownloadType.JointStatistic;
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
                    Type = convertType(item.Type),
                    CreateTime = item.AddedAt
                });
                var agency = Container.Resolve<IUserContract>().CurrentAgency(item.UserId);
                if (agency.Status)
                {
                    log.AgencyId = agency.Data.AgencyId;
                }
                log.CompleteTime = item.AddedAt;
                var result = _systemContract.CompleteDownload(log);
                if (result.Status)
                {
                    collection.Remove(Query.EQ("_id", item.Id));
                }
                Console.WriteLine(Utils.GetTimeNow() + " - " + result.ToJson());
            }
            Console.WriteLine("Complete");
        }
    }
}
