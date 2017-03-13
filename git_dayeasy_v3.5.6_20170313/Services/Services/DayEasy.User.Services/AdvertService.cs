using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.MongoDb;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace DayEasy.User.Services
{
    public partial class AdvertService : IAdvertContract
    {
        private readonly MongoCollection<MongoAdvert> _collection;
        private readonly MongoCollection<MongoAdvertCategory> _collectionCategory;

        public AdvertService()
        {
            _collection = new MongoManager().Collection<MongoAdvert>();
            _collectionCategory = new MongoManager().Collection<MongoAdvertCategory>();
        }

        public DResults<AdvertDto> Adverts(int index,int size,string category ="", string key="")
        {
            int total;
            List<AdvertDto> list;
            bool hasCategory = category.IsNotNullOrEmpty(), hasKey = key.IsNotNullOrEmpty();

            if (index < 0) index = 0;
            if (size < 5) size = 5;

            if (hasCategory || hasKey)
            {
                IMongoQuery query, queryCategory = null, queryKey = null;
                if (hasCategory)
                    queryCategory = Query.EQ("Category", category);
                if (hasKey)
                {
                    var reg = new Regex(key,RegexOptions.IgnoreCase);
                    queryKey = Query.Or(
                        Query.Matches("Name", reg),
                        Query.Matches("Text", reg),
                        Query.Matches("ForeignKey", reg));
                }
                
                if (hasCategory && hasKey)
                    query = Query.And(queryCategory, queryKey);
                else if (hasCategory)
                    query = queryCategory;
                else
                    query = queryKey;

                total = (int)_collection.Count(query);
                list = _collection.Find(query)
                .SetSortOrder(SortBy<MongoAdvert>.Descending(t => t.AddedAt))
                .SetSkip(index * size).SetLimit(size)
                .MapTo<List<AdvertDto>>();
            }
            else
            {
                total = (int)_collection.Count();
                list = _collection.FindAll()
                .SetSortOrder(SortBy<MongoAdvert>.Descending(t => t.AddedAt))
                .SetSkip(index * size).SetLimit(size)
                .MapTo<List<AdvertDto>>();
            }

            return DResult.Succ(list, total);

        }

        public List<AdvertDto> Adverts(List<string> ids)
        {
            return _collection.Find(Query.In("_id", ids.Select(t => new BsonString(t))))
                .MapTo<List<AdvertDto>>();
        }

        public DResult<AdvertDto> Advert(string id)
        {
            var item = _collection.FindOneById(id);
            if (item == null) return DResult.Error<AdvertDto>("图文广告不存在");
            var result = item.MapTo<AdvertDto>();
            return DResult.Succ(result);
        }

        public DResult<AdvertDto> Edit(AdvertDto dto)
        {
            if (dto == null) return DResult.Error<AdvertDto>("参数错误，请刷新重试");
            if (dto.Text.IsNullOrEmpty() && dto.ImageUrl.IsNullOrEmpty() && dto.ForeignKey.IsNullOrEmpty())
                return DResult.Error<AdvertDto>("广告文本、图片链接、关联ID至少填写一项");

            MongoAdvert advert;
            var isInsert = dto.Id.IsNullOrEmpty();

            if (isInsert)
            {
                dto.Id = IdHelper.Instance.Guid32;
                advert = new MongoAdvert {Id = dto.Id, AddedAt = Clock.Now};
            }
            else
            {
                advert = _collection.FindOneById(dto.Id);
                if (advert == null) return DResult.Error<AdvertDto>("没有查询到该图文广告");
            }

            advert.Name = dto.Name;
            advert.Text = dto.Text;
            advert.TextLink = dto.TextLink;
            advert.ImageUrl = dto.ImageUrl;
            advert.ForeignKey = dto.ForeignKey;
            advert.Category = dto.Category;
            advert.Index = dto.Index;

            if (isInsert)
            {
                _collection.Insert(advert);
            }
            else
            {
                _collection.Save(advert);
            }

            return DResult.Succ(dto);
        }

        public DResult Delete(string id)
        {
            _collection.Remove(Query.EQ("_id", id));
            return DResult.Success;
        }
    }
}
