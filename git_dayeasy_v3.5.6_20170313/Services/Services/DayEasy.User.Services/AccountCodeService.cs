using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.MongoDb;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace DayEasy.User.Services
{
    public class AccountCodeService : IAccountCodeContract
    {

        private readonly MongoCollection<MongoAccountCode> _collection;

        public AccountCodeService()
        {
            _collection = new MongoManager().Collection<MongoAccountCode>();
        }

        public void Edit(string account)
        {
            var item = Get(account);
            if (item != null)
            {
                item.Count += 1;
                item.Total += 1;
                item.Time = Clock.Now;
                _collection.Save(item);
                return;
            }
            _collection.Insert(new MongoAccountCode
            {
                Id = IdHelper.Instance.GetLongId(),
                Account = account,
                Count = 1,
                Total = 1,
                Time = Clock.Now
            });
        }

        public void Reset(string account)
        {
            var item = Get(account);
            if (item == null) return;
            item.Count = 0;
            _collection.Save(item);
        }

        public MongoAccountCode Get(string account)
        {
            return _collection.FindOne(Query.EQ("Account", account));
        }

    }
}
