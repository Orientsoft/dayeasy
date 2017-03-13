using System;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos
{
    /// <summary> 图文广告 </summary>
    [AutoMapFrom(typeof(MongoAdvert))]
    public class AdvertDto : DDto
    {
        /// <summary> ID </summary>
        public string Id { get; set; }

        /// <summary> 排序 </summary>
        public int Index { get; set; }

        /// <summary> 自定义分类ID </summary>
        public string Category { get; set; }

        /// <summary> 自定义别名 </summary>
        public string Name { get; set; }

        /// <summary> 关联数据 </summary>
        public string ForeignKey { get; set; }

        /// <summary> 广告文本 </summary>
        public string Text { get; set; }

        /// <summary> 文本链接 </summary>
        public string TextLink { get; set; }

        /// <summary> 广告图片链接 </summary>
        public string ImageUrl { get; set; }

        /// <summary> 添加时间 </summary>
        [MapFrom("AddedAt")]
        public DateTime CreateTime { get; set; }
    }

    /// <summary> 图文自定义分类 </summary>
    [AutoMapFrom(typeof(MongoAdvertCategory))]
    public class AdvertCategoryDto : DDto
    {
        /// <summary> ID </summary>
        public string Id { get; set; }

        /// <summary> 分类名称 </summary>
        public string CategoryName { get; set; }
    }
}
