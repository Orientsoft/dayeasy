using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.MongoDb;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace DayEasy.Statistic.Services
{
    public class SmsScoreNoticeServices : ISmsScoreNoticeContract
    {
        private readonly MongoCollection<MongoSmsScoreNotice> _collection;

        public SmsScoreNoticeServices()
        {
            _collection = new MongoManager().Collection<MongoSmsScoreNotice>();
        }

        public void Add(MongoSmsScoreNotice item)
        {
            if (item == null) return;
            var query = Query.And(Query.EQ("Batch", item.Batch),
                Query.EQ("PaperId", item.PaperId),
                Query.EQ("Mobile", item.Mobile));
            var exist = _collection.FindOne(query) != null;
            if (exist) return;
            _collection.Insert(item);
        }

        public void Add(List<MongoSmsScoreNotice> list)
        {
            if (list == null) return;
            list.ForEach(Add);
        }

        public List<MongoSmsScoreNotice> FindByBatch(string batch, string paperId)
        {
            var query = Query.And(Query.EQ("Batch", batch), Query.EQ("PaperId", paperId));
            return _collection.Find(query).ToList();
        }
    }
}
