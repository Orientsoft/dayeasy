using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core;

namespace DayEasy.Contracts
{
    /// <summary> 短信通知考试成绩 </summary>
    public interface ISmsScoreNoticeContract : IDependency
    {
        void Add(MongoSmsScoreNotice item);

        void Add(List<MongoSmsScoreNotice> list);

        List<MongoSmsScoreNotice> FindByBatch(string batch, string paperId);
    }
}
