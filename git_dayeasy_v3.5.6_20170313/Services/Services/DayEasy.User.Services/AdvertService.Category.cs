using System.Collections.Generic;
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using MongoDB.Driver.Builders;

namespace DayEasy.User.Services
{
    public partial class AdvertService
    {
        public DResults<AdvertCategoryDto> Categorys()
        {
            var list = _collectionCategory.FindAll()
                .SetSortOrder(SortBy<MongoAdvertCategory>.Descending(t => t.AddedAt))
                .MapTo<List<AdvertCategoryDto>>();
            return DResult.Succ(list, list.Count);
        }

        public DResult<AdvertCategoryDto> CategoryEdit(AdvertCategoryDto dto)
        {
            if (dto.CategoryName.IsNullOrEmpty())
                return DResult.Error<AdvertCategoryDto>("请设置分类名称");

            var isInsert = dto.Id.IsNullOrEmpty();
            if (isInsert)
            {
                dto.Id = IdHelper.Instance.Guid32;
                _collectionCategory.Insert(new MongoAdvertCategory
                {
                    Id = dto.Id,
                    CategoryName = dto.CategoryName,
                    AddedAt = Clock.Now
                });
            }
            else
            {
                var category = _collectionCategory.FindOneById(dto.Id);
                if (category == null)
                    return DResult.Error<AdvertCategoryDto>("没有查询到该分类");
                category.CategoryName = dto.CategoryName;
                _collectionCategory.Save(category);
            }
            return DResult.Succ(dto);
        }

        public DResult CategoryDelete(string id)
        {
            _collectionCategory.Remove(Query.EQ("_id", id));
            return DResult.Success;
        }
    }
}
