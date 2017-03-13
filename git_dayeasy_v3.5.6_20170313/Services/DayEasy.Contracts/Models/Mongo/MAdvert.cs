using System;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models.Mongo
{
    /// <summary> 图文广告数据源 </summary>
    public class MongoAdvert : DEntity<string>
    {
        /// <summary> ID </summary>
        public override string Id { get; set; }

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

        /// <summary> 文本超链接 </summary>
        public string TextLink { get; set; }

        /// <summary> 广告图片链接 </summary>
        public string ImageUrl { get; set; }

        /// <summary> 添加时间 </summary>
        public DateTime AddedAt { get; set; }
    }

    /// <summary>
    /// 图文广告分类
    /// </summary>
    public class MongoAdvertCategory : DEntity<string>
    {
        /// <summary> ID </summary>
        public override string Id { get; set; }

        /// <summary> 分类名称 </summary>
        public string CategoryName { get; set; }

        /// <summary> 添加时间 </summary>
        public DateTime AddedAt { get; set; }
    }
}
